using System.Collections.Generic;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
#else
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
#endif

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextService
    {
        string BuildValue(IPublishedElement owner, IPublishedPropertyType propertyType);
        Dictionary<string, object> BuildDictionary(int nodeId, string propertyAlias);
    }
}
