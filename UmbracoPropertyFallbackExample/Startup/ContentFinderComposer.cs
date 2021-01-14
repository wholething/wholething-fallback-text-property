using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using UmbracoPropertyFallbackExample.ContentFinders;

namespace UmbracoPropertyFallbackExample.Startup
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)] 
    public class FallbackContentFinderComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.ContentFinders().Append<FallbackPropertyContentFinder>();
        }
    }
}