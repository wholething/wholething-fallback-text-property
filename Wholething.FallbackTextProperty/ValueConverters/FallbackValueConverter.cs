using System;
using Wholething.FallbackTextProperty.Services;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
#else
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
#endif

namespace Wholething.FallbackTextProperty.ValueConverters
{
    public class FallbackTextPropertyValueConverter : IPropertyValueConverter
    {
        private readonly IFallbackTextService _fallbackTextService;

        public FallbackTextPropertyValueConverter(IFallbackTextService fallbackTextService)
        {
            _fallbackTextService = fallbackTextService;
        }

        public bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias == "FallbackTextstring" || propertyType.EditorAlias == "FallbackTextarea";
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
            var value = source as string;

            if (value == "<none>")
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return _fallbackTextService.BuildValue(owner, propertyType);
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