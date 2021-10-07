﻿using Wholething.FallbackTextProperty.Services;
using Wholething.FallbackTextProperty.Services.Impl;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
#else
using Umbraco.Core.Composing;
#endif

namespace Wholething.FallbackTextProperty.Composers
{
#if NET5_0_OR_GREATER
    public class FallbackTextComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IFallbackTextService, FallbackTextService>();
        }
    }
#else
    public class FallbackTextComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register(typeof(IFallbackTextService), typeof(FallbackTextService), Lifetime.Singleton);
        }
    }
#endif
}
