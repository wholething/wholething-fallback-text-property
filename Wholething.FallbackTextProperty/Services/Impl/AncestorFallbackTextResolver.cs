﻿using System;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
#else
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class AncestorFallbackTextResolver : FallbackTextResolver
    {
        protected override string FunctionName => "ancestor";

        public override void CheckArguments(string[] args)
        {
            if (args.Length > 1)
            {
                throw new ArgumentException($"{FunctionName} expects 0 or 1 arguments");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            if (args.Length == 1)
            {
                return context.Owner.Ancestor(args[0]);
            }
            return context.Owner.Ancestor();
        }
    }
}