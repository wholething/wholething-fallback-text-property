using System.Collections.Generic;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextService
    {
        string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType, string culture);
        Dictionary<string, object> BuildDictionary(int nodeId, string propertyAlias, string culture);
    }
}
