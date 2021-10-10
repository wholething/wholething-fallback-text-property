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
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextService : IFallbackTextService
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private readonly IEnumerable<IFallbackTextResolver> _resolvers;

        private const string IdReferencePattern = @"{{([0-9]+):.+}}";
        private const string FunctionReferencePattern = @"{{(\w+)(\((.+)\))?:.+}}";

        public FallbackTextService(IPublishedSnapshotAccessor publishedSnapshotAccessor, IEnumerable<IFallbackTextResolver> resolvers)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _resolvers = resolvers;
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

            foreach (var referencedNode in referencedNodes)
            {
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

            // There is some quirk of the Mustache implementation that means a variable name cannot
            // start with a number!
            template = Regex.Replace(
                template,
                IdReferencePattern,
                m => $"node{m.Groups[1].Value}"
            );

            return template;
        }

        private List<IPublishedContent> GetAllReferencedNodes(string template, IPublishedContent owner)
        {
            var nodes = new List<IPublishedContent>();

            // TODO: Not sure if this is necessary - shouldn't the owner always be an IPublishedContent?
            if (owner != null)
            {
                nodes.AddRange(GetFunctionReferences(template, owner));
            }

            var idReferences = GetIdReferences(template);
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            nodes.AddRange(
                idReferences.Select(id => publishedSnapshot.Content.GetById(id))
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

        private List<IPublishedContent> GetFunctionReferences(string template, IPublishedContent owner)
        {
            var regex = new Regex(FunctionReferencePattern);
            var matches = regex.Matches(template);

            var references = new List<FallbackTextFunctionReference>();
            foreach (Match match in matches)
            {
                var args = match.Groups.Count != 3 ? 
                    new string[0] :
                    match.Groups[3].Value
                        .Split(',')
                        .Select(s => s.Trim())
                        .ToArray();

                references.Add(new FallbackTextFunctionReference
                {
                    Function = match.Groups[1].Value,
                    Args = args
                });
            }
            references = references.Distinct().ToList();

            var resolverContext = new FallbackTextResolverContext(owner);

            var nodes = references
                .Select(r => TryResolve(r, resolverContext))
                .Where(n => n != null)
                .ToList();

            return nodes;
        }

        private IPublishedContent TryResolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context)
        {
            var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(reference));
            return resolver?.Resolve(reference, context);
        }
    }
}
