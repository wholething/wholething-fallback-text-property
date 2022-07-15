using System;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class ParentFallbackTextResolver : FallbackTextResolver
    {
        public ParentFallbackTextResolver(IFallbackTextLoggerService logger) : base(logger)
        {
        }


        protected override string FunctionName => "parent";
        protected override bool RequireContent => true;

        public override void CheckArguments(string[] args, FallbackTextResolverContext context)
        {
            base.CheckArguments(args, context);

            if (args.Length > 0)
            {
                throw new ArgumentException("Did not expect any arguments");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            return context.Content.Parent;
        }
    }
}
