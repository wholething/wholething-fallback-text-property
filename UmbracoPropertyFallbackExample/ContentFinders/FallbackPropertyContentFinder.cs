using Umbraco.Web.Routing;

namespace UmbracoPropertyFallbackExample.ContentFinders
{
    public class FallbackPropertyContentFinder : IContentFinder
    {
        public bool TryFindContent(PublishedRequest request)
        {
            return false;
        }
    }
}