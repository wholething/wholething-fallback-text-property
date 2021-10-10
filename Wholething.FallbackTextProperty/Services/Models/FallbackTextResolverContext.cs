#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services.Models
{
    public class FallbackTextResolverContext
    {
        public FallbackTextResolverContext(IPublishedContent owner)
        {
            Owner = owner;
        }

        public IPublishedContent Owner { get; set; }
    }
}
