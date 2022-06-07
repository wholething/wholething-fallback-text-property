using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Wholething.FallbackTextProperty.Extensions;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
#else
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextService : IFallbackTextService
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private readonly IEnumerable<IFallbackTextResolver> _resolvers;
        private readonly IFallbackTextReferenceParser _referenceParser;

        private const string IdReferencePattern = @"{{(?>node)?([0-9]+):(\w+)}}";

        public FallbackTextService(IPublishedSnapshotAccessor publishedSnapshotAccessor, IEnumerable<IFallbackTextResolver> resolvers, IFallbackTextReferenceParser referenceParser)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _resolvers = resolvers;
            _referenceParser = referenceParser;
        }

        public string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType, string culture)
        {
            var template = GetTemplate(propertyType);
            template = PreprocessTemplate(template);

            var dictionary = BuildDictionary(owner, propertyType, culture);

            var compiled = Handlebars.Compile(template);

            dictionary = PreprocessDictionary(dictionary);

            return WebUtility.HtmlDecode(compiled(dictionary));
        }

        private Dictionary<string, object> PreprocessDictionary(Dictionary<string, object> dictionary)
        {
            var outDictionary = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                var outKey = key;
                if (char.IsDigit(key[0]))
                {
                    outKey = $"node{key}";
                }
                outDictionary[outKey] = dictionary[key];
            }
            return outDictionary;
        }

        private string PreprocessTemplate(string template)
        {
            // There is some quirk of the Mustache implementation that means a variable name cannot
            // start with a number!
            return Regex.Replace(
                template,
                IdReferencePattern,
                m => $"{{{{node{m.Groups[1].Value}:{m.Groups[2].Value}}}}}"
            );
        }

        public Dictionary<string, object> BuildDictionary(int nodeId, string propertyAlias, string culture)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var node = publishedSnapshot.Content.GetById(nodeId);

            if (node == null) return new Dictionary<string, object>();

            return BuildDictionary(node, GetPropertyType(node, propertyAlias), culture);
        }

        public Dictionary<string, object> BuildDictionary(Guid nodeId, string propertyAlias, string culture)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var node = publishedSnapshot.Content.GetById(nodeId);

            if (node == null) return new Dictionary<string, object>();

            return BuildDictionary(node, GetPropertyType(node, propertyAlias), culture);
        }

        private IPublishedPropertyType GetPropertyType(IPublishedElement node, string propertyAlias)
        {
            var propertyType = node.Properties.FirstOrDefault(p => p.Alias == propertyAlias)?.PropertyType;

            if (propertyType == null)
            {
                throw new ArgumentException($"No property \"{propertyAlias}\" exists on node {node.Key} ({node.ContentType.Alias})");
            }

            return propertyType;
        }

        private Dictionary<string, object> BuildDictionary(IPublishedElement owner, IPublishedPropertyType propertyType, string culture)
        {
            var template = GetTemplate(propertyType);
            var dictionary = new Dictionary<string, object>();

            if (owner is IPublishedContent node)
            {
                dictionary.Add("name", node.Name);
            }

            foreach (var publishedProperty in owner.Properties)
            {
                var propertyValue = publishedProperty.GetSourceValueWithCulture(culture);
                if (propertyValue is string strValue)
                {
                    dictionary[publishedProperty.Alias] = strValue;
                }
            }
            
            var referencedNodes = GetAllReferencedNodes(template, owner as IPublishedContent);

            foreach (var (key, referencedNode) in referencedNodes)
            {
                dictionary.Add($"{key}:name", referencedNode.Name);

                foreach (var property in referencedNode.Properties)
                {
                    var propertyValue = property.GetSourceValueWithCulture(culture);
                    if (propertyValue is string strPropertyValue)
                    {
                        dictionary[$"{key}:{property.Alias}"] = strPropertyValue;
                    }
                }
            }

            return dictionary;
        }

        private string GetTemplate(IPublishedPropertyType propertyType)
        {
            var template = (string)((Dictionary<string, object>)propertyType.DataType.Configuration)["fallbackTemplate"];
            return template;
        }

        private Dictionary<string, IPublishedContent> GetAllReferencedNodes(string template, IPublishedElement owner)
        {
            var nodes = new Dictionary<string, IPublishedContent>();

            if (owner != null)
            {
                nodes.AddRange(GetFunctionReferences(template, owner));
            }

            var idReferences = GetIdReferences(template);
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            nodes.AddRange(
                idReferences
                    .ToDictionary(
                        id => $"{id}", 
                        id => publishedSnapshot.Content.GetById(id)
                    )
            );

            return nodes;
        }

        private List<int> GetIdReferences(string template)
        {
            var regex = new Regex(IdReferencePattern);
            var matches = regex.Matches(template);

            var nodeIds = new List<int>();
            foreach (Match match in matches)
            {
                nodeIds.Add(int.Parse(match.Groups[1].Value));
            }

            return nodeIds;
        }

        private Dictionary<string, IPublishedContent> GetFunctionReferences(string template, IPublishedElement owner)
        {
            // TODO: Not sure if this is necessary - shouldn't the owner always be an IPublishedContent?
            if (owner == null)
            {
                return new Dictionary<string, IPublishedContent>();
            }

            var references = _referenceParser.Parse(template);

            var resolverContext = new FallbackTextResolverContext(owner);

            // Need to keep the key with the resolved node
            var nodes = references
                .ToDictionary(x => x.Key, x => TryResolve(x, resolverContext))
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);

            return nodes;
        }

        private IPublishedContent TryResolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context)
        {
            var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(reference));
            return resolver?.Resolve(reference, context);
        }
    }
}
