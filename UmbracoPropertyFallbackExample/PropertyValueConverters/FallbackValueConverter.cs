using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace UmbracoPropertyFallbackExample.PropertyValueConverters
{
    public class FallbackValueConverter : IPropertyValueConverter
    {
        private readonly IContentService _contentService;
        private readonly IDataTypeService _dataTypeService;

        public FallbackValueConverter(IDataTypeService dataTypeService, IContentService contentService)
        {
            _dataTypeService = dataTypeService;
            _contentService = contentService;
        }

        public bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias == "FallbackTextstring";
        }

        public bool? IsValue(object value, PropertyValueLevel level)
        {
            switch (level)
            {
                case PropertyValueLevel.Source:
                    return value != null && (!(value is string) || string.IsNullOrWhiteSpace((string)value) == false);
                default:
                    throw new NotSupportedException($"Invalid level: {level}.");
            }
        }

        public Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            return typeof(string);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Elements;
        }

        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source,
            bool preview)
        {
            var fallbackValue = JsonConvert.DeserializeObject<FallbackValue>(source.ToString());

            if (fallbackValue.UseValue)
            {
                return fallbackValue.Value;
            }

            var dataType = _dataTypeService.GetByEditorAlias(propertyType.EditorAlias).First();

            var template = (string) ((Dictionary<string, object>) dataType.Configuration)["fallbackTemplate"];

            var dictionary = new Dictionary<string, string>();

            foreach (var publishedProperty in owner.Properties)
            {
                var value = publishedProperty.GetSourceValue();
                if (value != null && value is string strValue)
                {
                    dictionary[publishedProperty.Alias] = strValue;
                }
            }

            var otherNodeIds = GetOtherNodeIds(template);
            foreach (var nodeId in otherNodeIds)
            {
                var node = _contentService.GetById(nodeId);

                // There is some quick of the Mustache implementation that means a variable name cannot
                // start with a number!
                template = template.Replace($"{nodeId}", $"node{nodeId}");

                foreach (var property in node.Properties)
                {
                    var value = property.GetValue();
                    if (value != null && value is string strValue)
                    {
                        dictionary[$"node{node.Id}:{property.Alias}"] = strValue;
                    }
                }
            }

            var compiled = Handlebars.Compile(template);

            return compiled(dictionary); 
        }

        private List<int> GetOtherNodeIds(string template)
        {
            var regex = new Regex(@"([0-9]+):");
            var matches = regex.Matches(template);

            var nodeIds = new List<int>();
            foreach (Match match in matches)
            {
                nodeIds.Add(int.Parse(match.Groups[1].Value));
            }

            return nodeIds;
        }

        public object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter;
        }

        public object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return inter?.ToString();
        }
    }
}