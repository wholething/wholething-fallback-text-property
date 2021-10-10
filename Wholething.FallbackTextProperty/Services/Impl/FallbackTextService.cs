using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
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

        private bool TryParseReference(string reference, out FallbackTextReference parsedReference)
        {
            try
            {
                var parts = reference.Split('(');
                var function = parts[0];
                var args = parts[1]
                    .Substring(0, parts[1].Length - 1)
                    .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim(new[] { ' ', ')' }))
                    .ToArray();

                parsedReference = new FallbackTextReference
                {
                    Function = function,
                    Args = args
                };
                return true;
            }
            catch (Exception)
            {
                parsedReference = null;
                return false;
            }
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

            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var referencedNodes = GetNodeReferences(template);
            foreach (var referencedNodeId in referencedNodes)
            {
                var referencedNode = publishedSnapshot.Content.GetById(referencedNodeId);

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

        private List<int> GetNodeReferences(string template)
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

        private List<IPublishedContent> GetOtherReferences(string template, IPublishedContent owner)
        {
            var regex = new Regex(FunctionReferencePattern);
            var matches = regex.Matches(template);

            var references = new List<string>();
            foreach (Match match in matches)
            {
                references.Add(match.Groups[1].Value);
            }
            references = references.Distinct().ToList();

            var resolverContext = new FallbackTextResolverContext(owner);

            var nodes = references
                .Select(r => TryResolve(r, resolverContext))
                .Where(n => n != null)
                .ToList();

            return nodes;
        }

        private IPublishedContent TryResolve(string reference, FallbackTextResolverContext context)
        {
            var parsed = TryParseReference(reference, out var parsedReference);
            if (!parsed)
            {
                return null;
            }

            var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(parsedReference));

            return resolver?.Resolve(parsedReference, context);
        }
    }
}
