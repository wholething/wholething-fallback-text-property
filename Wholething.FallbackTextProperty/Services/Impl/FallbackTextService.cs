using System.Collections.Generic;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
#else
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextService : IFallbackTextService
    {
        private readonly IContentService _contentService;

        public FallbackTextService(IContentService contentService)
        {
            _contentService = contentService;
        }

        private List<int> GetReferencedNodes(string template)
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

        public string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType)
        {
            var template = GetTemplate(propertyType);

            var dictionary = BuildDictionary(owner, propertyType);

            var compiled = Handlebars.Compile(template);

            return compiled(dictionary);
        }

        public Dictionary<string, object> BuildDictionary(int nodeId, string propertyAlias)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, object> BuildDictionary(IPublishedElement owner, IPublishedPropertyType propertyType)
        {
            var template = GetTemplate(propertyType);
            var dictionary = new Dictionary<string, object>();

            if (owner is IPublishedContent node)
            {
                dictionary.Add("name", node.Name);
            }

            foreach (var publishedProperty in owner.Properties)
            {
                var propertyValue = publishedProperty.GetSourceValue();
                if (propertyValue is string strValue)
                {
                    dictionary[publishedProperty.Alias] = strValue;
                }
            }

            var referencedNodes = GetReferencedNodes(template);
            foreach (var referencedNodeId in referencedNodes)
            {
                var referencedNode = _contentService.GetById(referencedNodeId);

                dictionary.Add($"node{referencedNode.Id}:name", referencedNode.Name);

                foreach (var property in referencedNode.Properties)
                {
                    var propertyValue = property.GetValue();
                    if (propertyValue != null && propertyValue is string strValue)
                    {
                        dictionary[$"node{referencedNode.Id}:{property.Alias}"] = strValue;
                    }
                }
            }

            return dictionary;
        }

        private string GetTemplate(IPublishedPropertyType propertyType)
        {
            var template = (string)((Dictionary<string, object>)propertyType.DataType.Configuration)["fallbackTemplate"];
            var referencedNodes = GetReferencedNodes(template);
            // There is some quirk of the Mustache implementation that means a variable name cannot
            // start with a number!
            referencedNodes.ForEach(referencedNodeId =>
            {
                template = template.Replace($"{referencedNodeId}", $"node{referencedNodeId}");
            });
            return template;
        }
    }
}
