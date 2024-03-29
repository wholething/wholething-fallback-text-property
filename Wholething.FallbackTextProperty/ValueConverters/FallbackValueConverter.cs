﻿using System;
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
        private readonly IVariationContextAccessor _variationContextAccessor;

        public FallbackTextPropertyValueConverter(IFallbackTextService fallbackTextService, IVariationContextAccessor variationContextAccessor)
        {
            _fallbackTextService = fallbackTextService;
            _variationContextAccessor = variationContextAccessor;
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

            var culture = _variationContextAccessor?.VariationContext?.Culture;

            return _fallbackTextService.BuildValue(owner, propertyType, culture);
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