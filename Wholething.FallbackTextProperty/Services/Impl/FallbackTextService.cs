using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using HandlebarsDotNet;
using Wholething.FallbackTextProperty.Extensions;
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

        public FallbackTextService(IPublishedSnapshotAccessor publishedSnapshotAccessor, IEnumerable<IFallbackTextResolver> resolvers)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _resolvers = resolvers;
        }

        private List<int> GetNodeReferences(string template)
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

        private List<IPublishedContent> GetOtherReferences(string template)
        {
            var regex = new Regex(@"([A-z()]+):");
            var matches = regex.Matches(template);

            var references = new List<string>();
            foreach (Match match in matches)
            {
                references.Add(match.Groups[1].Value);
            }
            references = references.Distinct().ToList();

            var nodes = references.Select(TryResolve).Where(n => n != null).ToList();
            return nodes;
        }

        private IPublishedContent TryResolve(string reference)
        {
            var parts = reference.Split('(');
            var function = parts[0];
            var args = parts[1]
                .Substring(0, parts[1].Length - 1)
                .Split(",", StringSplitOptions.RemoveEmptyEntries);

            IFallbackTextResolver resolver = _resolvers.FirstOrDefault(r => r.CanResolve());
            
            throw new System.NotImplementedException();
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
            var referencedNodes = GetNodeReferences(template);
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
