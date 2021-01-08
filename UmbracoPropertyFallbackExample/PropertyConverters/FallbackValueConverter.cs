using System;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace UmbracoPropertyFallbackExample.PropertyConverters
{
    public class FallbackValueConverter : PropertyValueConverterBase, IPropertyValueConverter
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias == "FallbackTextstring";
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            return typeof(FallbackValue);
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source,
            bool preview)
        {
            return new FallbackValue();
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            return new FallbackValue();
        }
    }
}