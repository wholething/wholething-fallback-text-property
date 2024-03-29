﻿using System.Collections.Generic;
using System.Linq;
using Wholething.FallbackTextProperty.Services;
using Wholething.FallbackTextProperty.Services.Impl;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
#else
using System.Collections;
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
            builder.Services.AddSingleton<IFallbackTextReferenceParser, FallbackTextReferenceParser>();
            builder.Services.AddSingleton<IFallbackTextLoggerService, FallbackTextLoggerService>();

            builder.Services.AddSingleton<IFallbackTextResolver, ParentFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, RootFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, AncestorFallbackTextResolver>();
            builder.Services.AddSingleton<IFallbackTextResolver, UrlFallbackTextResolver>();
        }
    }
#else
    public class FallbackTextComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register(typeof(IFallbackTextService), typeof(FallbackTextService), Lifetime.Singleton);
            composition.Register(typeof(IFallbackTextReferenceParser), typeof(FallbackTextReferenceParser), Lifetime.Singleton);
            composition.Register(typeof(IFallbackTextLoggerService), typeof(FallbackTextLoggerService), Lifetime.Singleton);
        }
    }
#endif
}
