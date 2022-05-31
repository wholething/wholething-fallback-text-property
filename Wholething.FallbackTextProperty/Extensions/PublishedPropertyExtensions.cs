using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Wholething.FallbackTextProperty.Extensions
{
    public static class PublishedPropertyExtensions
    {
        /// <summary>
        /// Get property source value with culture (if the property does not vary by culture, culture is ignored)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static object GetSourceValueWithCulture(this IPublishedProperty property, string culture)
        {
            if (property.PropertyType.VariesByCulture())
            {
                return property.GetSourceValue(culture);
            }
            else
            {
                return property.GetSourceValue();
            }
        }
    }
}
