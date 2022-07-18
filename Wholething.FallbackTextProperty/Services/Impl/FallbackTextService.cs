using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using Wholething.FallbackTextProperty.Extensions;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Models.Blocks;
#else
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Blocks;
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
        
        private readonly IDataTypeService _dataTypeService;
        
        private readonly IFallbackTextLoggerService _logger;

        private const string IdReferencePattern = @"{{(?>node)?([0-9]+):(\w+)}}";
        private const string GuidReferencePattern = @"(?im)[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}";

        public FallbackTextService(IPublishedSnapshotAccessor publishedSnapshotAccessor, IEnumerable<IFallbackTextResolver> resolvers, 
            IFallbackTextReferenceParser referenceParser, IDataTypeService dataTypeService, IFallbackTextLoggerService logger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _resolvers = resolvers;
            _referenceParser = referenceParser;
            _dataTypeService = dataTypeService;
            _logger = logger;
        }

        public string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType, string culture)
        {
            var template = GetTemplate(propertyType.DataType.Configuration);
            template = PreprocessTemplate(template);

            var dictionary = BuildDictionary(owner, propertyType.DataType.Configuration, culture);
            dictionary = PreprocessDictionary(dictionary);

            var handlebars = Handlebars.Create(new HandlebarsConfiguration()
            {
                ThrowOnUnresolvedBindingExpression = true
            });

            try
            {
                var compiled = handlebars.Compile(template);
                return WebUtility.HtmlDecode(compiled(dictionary));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallback text template value missing (node couldn't be found or function didn't resolve)");

                // Do best effort rendering
                var compiled = Handlebars.Compile(template);
                return WebUtility.HtmlDecode(compiled(dictionary));
            }
        }

        private Dictionary<string, object> PreprocessDictionary(Dictionary<string, object> dictionary)
        {
            var outDictionary = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                var outKey = PreprocessKey(key);
                outDictionary[outKey] = dictionary[key];
            }
            return outDictionary;
        }

        private string PreprocessKey(string key)
        {
            if (char.IsDigit(key[0]))
            {
                return $"node{key}";
            }
            else
            {
                return key.StripNonKeyChars();
            }
        }

        private string PreprocessTemplate(string template)
        {
            // There is some quirk of the Mustache implementation that means a variable name cannot
            // start with a number!
            template = Regex.Replace(
                template,
                IdReferencePattern,
                m => $"{{{{node{m.Groups[1].Value}:{m.Groups[2].Value}}}}}"
            );

            template = Regex.Replace(
                template,
                Constants.Regex.FunctionReferencePattern,
                m =>
                {
                    var key = $"{m.Groups[1].Value}:{m.Groups[5].Value}";
                    return $"{{{{{key.StripNonKeyChars()}}}}}";
                });

            return template;
        }

        public Dictionary<string, object> BuildDictionary(Guid nodeId, Guid? blockId, Guid dataTypeAlias, string culture)
        {
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            var node = publishedSnapshot.Content.GetById(nodeId);
            var block = !blockId.HasValue ? null : GetBlockFromNode(node, blockId.Value);

            if (node == null) return new Dictionary<string, object>();

            return BuildDictionary(blockId.HasValue ? block : node, GetDataTypeConfiguration(dataTypeAlias), culture);
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

            return null;
        }

        private object GetDataTypeConfiguration(Guid dataTypeKey)
        {
            return _dataTypeService.GetDataType(dataTypeKey).Configuration;
        }

        private Dictionary<string, object> BuildDictionary(IPublishedElement owner, object dataTypeConfiguration, string culture)
        {
            var template = GetTemplate(dataTypeConfiguration);
            var dictionary = new Dictionary<string, object>();

            if (owner is IPublishedContent node)
            {
                dictionary.Add("name", node.Name);
            }

            if (owner != null)
            {
                foreach (var publishedProperty in owner.Properties)
                {
                    var propertyValue = publishedProperty.GetSourceValueWithCulture(culture);
                    if (propertyValue is string strValue)
                    {
                        dictionary[publishedProperty.Alias] = strValue;
                    }
                }
            }

            var referencedNodes = GetAllReferencedNodes(template, owner);

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
            
            nodes.AddRange(GetFunctionReferences(template, owner));
            
            var publishedSnapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();

            var idReferences = GetIdReferences(template);
            nodes.AddRange(
                idReferences
                    .ToDictionary(
                        id => id.ToString(), 
                        id => publishedSnapshot.Content.GetById(id)
                    )
            );

            var guidReferences = GetGuidReferences(template);
            nodes.AddRange(
                guidReferences
                    .ToDictionary(
                        id => id.ToString(),
                        id => publishedSnapshot.Content.GetById(id)
                    )
            );

            return nodes;
        }

        private List<int> GetIdReferences(string template)
        {
            var regex = new Regex(IdReferencePattern);
            var matches = regex.Matches(template);

            var ids = new List<int>();
            foreach (Match match in matches)
            {
                ids.Add(int.Parse(match.Groups[1].Value));
            }

            return ids;
        }

        private List<Guid> GetGuidReferences(string template)
        {
            var regex = new Regex(GuidReferencePattern);
            var matches = regex.Matches(template);

            var guids = new List<Guid>();
            foreach (Match match in matches)
            {
                guids.Add(Guid.Parse(match.Groups[0].Value));
            }

            return guids;
        }

        private Dictionary<string, IPublishedContent> GetFunctionReferences(string template, IPublishedElement owner)
        {
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
            var resolver = _resolvers.FirstOrDefault(r => r.CanResolve(reference, context));
            return resolver?.Resolve(reference, context);
        }
    }
}
