using System;
using System.Collections.Generic;
using System.Linq;
using Mustache;
using Newtonsoft.Json;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace UmbracoPropertyFallbackExample.PropertyValueConverters
{
    public class FallbackValueConverter : IPropertyValueConverter
    {
        private readonly IUmbracoContextFactory _contextFactory;
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;

        public FallbackValueConverter(/*IUmbracoContextFactory contextFactory, IContentTypeService contentTypeService,*/ IDataTypeService dataTypeService)
        {
            //_contextFactory = contextFactory;
            //_contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
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

            var dataType = _dataTypeService.GetByEditorAlias(propertyType.EditorAlias).First();

            var template = (string) ((Dictionary<string, object>) dataType.Configuration)["fallbackTemplate"];

            template = template.Replace(':', '-');

            var dictionary = new Dictionary<string, string>
            {
                { "pageTitle", "pageTitle" }
            };

            var compiler = new FormatCompiler();
            var generator = compiler.Compile(template);
            var result = generator.Render(dictionary);

            return fallbackValue.Value;
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