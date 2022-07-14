using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Wholething.FallbackTextProperty.Extensions;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Models.Blocks;
#else
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PublishedCache;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextService : IFallbackTextService
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        private readonly IEnumerable<IFallbackTextResolver> _resolvers;
        private readonly IFallbackTextReferenceParser _referenceParser;

        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;

        private const string IdReferencePattern = @"{{(?>node)?([0-9]+):(\w+)}}";

        public FallbackTextService(IPublishedSnapshotAccessor publishedSnapshotAccessor, IEnumerable<IFallbackTextResolver> resolvers, IFallbackTextReferenceParser referenceParser, IContentTypeService contentTypeService, IDataTypeService dataTypeService)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _resolvers = resolvers;
            _referenceParser = referenceParser;
            _contentTypeService = contentTypeService;
            _dataTypeService = dataTypeService;
        }

        public string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType, string culture)
        {
            var template = GetTemplate(propertyType.DataType.Configuration);
            template = PreprocessTemplate(template);

            var dictionary = BuildDictionary(owner, propertyType.DataType.Configuration, culture);

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

        public Dictionary<string, object> BuildDictionary(Guid nodeId, Guid? blockId, string propertyAlias, string culture)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var node = publishedSnapshot.Content.GetById(nodeId);
            var block = !blockId.HasValue ? null : GetBlockFromNode(node, blockId.Value);

            if (node == null) return new Dictionary<string, object>();

            return BuildDictionary(node, GetDataTypeConfiguration(block ?? node, propertyAlias), culture);
        }

        private IPublishedElement GetBlockFromNode(IPublishedContent node, Guid blockId)
        {
            foreach (var publishedProperty in node.Properties)
            {
                if (publishedProperty.PropertyType.ClrType == typeof(BlockListModel))
                {
                    var blockList = (BlockListModel) publishedProperty.GetValue();
                    foreach (var blockListItem in blockList)
                    {
                        if (blockListItem.Content.Key == blockId) return blockListItem.Content;
                    }
                }
            }

            throw new ArgumentException($"Couldn't find block {blockId} on node {node.Name}");
        }

        private object GetDataTypeConfiguration(IPublishedElement node, string propertyAlias)
        {
            if (propertyAlias.Contains("__"))
            {
                var propertyAliases = propertyAlias.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var propertyType = node.Properties.FirstOrDefault(p => p.Alias == propertyAliases[0])?.PropertyType;

                var config = propertyType.DataType.Configuration as NestedContentConfiguration;

                var contentTypes = config.ContentTypes;

                foreach (var contentTypeStub in contentTypes)
                {
                    var contentType = _contentTypeService.Get(contentTypeStub.Alias);
                    var elementPropertyType = contentType.PropertyGroups
                        .SelectMany(x => x.PropertyTypes)
                        .FirstOrDefault(p => p.Alias == propertyAliases[1]);
                    if (elementPropertyType != null)
                    {
                        return _dataTypeService.GetDataType(elementPropertyType.DataTypeKey).Configuration;
                    }
                }
                
                throw new ArgumentException($"No element property \"{propertyAlias}\" exists on node {node.Key} ({node.ContentType.Alias})");
            }
            else
            {
                var propertyType = node.Properties.FirstOrDefault(p => p.Alias == propertyAlias)?.PropertyType;

                if (propertyType == null)
                {
                    throw new ArgumentException($"No property \"{propertyAlias}\" exists on node {node.Key} ({node.ContentType.Alias})");
                }

                return propertyType.DataType.Configuration;
            }
        }

        private Dictionary<string, object> BuildDictionary(IPublishedElement owner, object dataTypeConfiguration, string culture)
        {
            var template = GetTemplate(dataTypeConfiguration);
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
                if (referencedNode == null) continue;

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

        private string GetTemplate(object configuration)
        {
            var template = (string)((Dictionary<string, object>)configuration)["fallbackTemplate"];
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
            // TODO: We want support elements/blocks but currently we don't
            if (!(owner is IPublishedContent))
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
