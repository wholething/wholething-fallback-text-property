using Umbraco.Core.Models.PublishedContent;

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
