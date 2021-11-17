using System.Collections.Generic;
using System.Linq;
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

        public string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType)
        {
            var template = GetTemplate(propertyType);

            var dictionary = BuildDictionary(owner, propertyType);

            var compiled = Handlebars.Compile(template);

            return compiled(dictionary);
        }

        public Dictionary<string, object> BuildDictionary(int nodeId, string propertyAlias)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var node = publishedSnapshot.Content.GetById(nodeId);

            if (node == null) return new Dictionary<string, object>();

            var propertyType = node.Properties.First(p => p.Alias == propertyAlias).PropertyType;

            return BuildDictionary(node, propertyType);
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
            
            var referencedNodes = GetAllReferencedNodes(template, owner as IPublishedContent);

            foreach (var (key, referencedNode) in referencedNodes)
            {
                dictionary.Add($"{key}:name", referencedNode.Name);

                foreach (var property in referencedNode.Properties)
                {
                    var propertyValue = property.GetValue();
                    if (propertyValue is string strValue)
                    {
                        dictionary[$"{key}:{property.Alias}"] = strValue;
                    }
                }
            }

            return dictionary;
        }

        private string GetTemplate(IPublishedPropertyType propertyType)
        {
            var template = (string)((Dictionary<string, object>)propertyType.DataType.Configuration)["fallbackTemplate"];

            // There is some quirk of the Mustache implementation that means a variable name cannot
            // start with a number!
            template = Regex.Replace(
                template,
                IdReferencePattern,
                m => $"{{{{node{m.Groups[1].Value}:{m.Groups[2].Value}}}}}"
            );

            return template;
        }

        private Dictionary<string, IPublishedContent> GetAllReferencedNodes(string template, IPublishedContent owner)
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
                        id => $"node{id}", 
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

        private Dictionary<string, IPublishedContent> GetFunctionReferences(string template, IPublishedContent owner)
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
