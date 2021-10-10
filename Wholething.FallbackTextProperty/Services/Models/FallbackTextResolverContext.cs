using Umbraco.Core.Models.PublishedContent;

namespace Wholething.FallbackTextProperty.Services.Models
{
    public class FallbackTextResolverContext
    {
        public IPublishedContent Owner { get; set; }
    }
}
