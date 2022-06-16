#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services.Models
{
    public class FallbackTextResolverContext
    {
        public FallbackTextResolverContext(IPublishedElement element)
        {
            Element = element;
        }

        public IPublishedElement Element { get; set; }
        public IPublishedContent Content => Element as IPublishedContent;
    }
}
