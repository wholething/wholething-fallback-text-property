using Microsoft.Extensions.DependencyInjection;
using Wholething.FallbackTextProperty.Services;
using Wholething.FallbackTextProperty.Services.Impl;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
#else
using Umbraco.Core.Composing;
#endif

namespace Wholething.FallbackTextProperty.Composers
{
    public class FallbackTextComposer : IComposer
    {
#if NET5_0_OR_GREATER
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IFallbackTextService, FallbackTextService>();
        }
#else
        public void Compose(Composition composition)
        {
            composition.RegisterFor<IFallbackTextService, FallbackTextService>();
        }
#endif
    }
}
