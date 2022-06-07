using System;
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
    public class RootFallbackTextResolver : FallbackTextResolver
    {
        protected override string FunctionName => "root";

        public override void CheckArguments(string[] args)
        {
            if (args.Length > 0)
            {
                throw new ArgumentException("Did not expect any arguments");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            return context.Element.Root();
        }
    }
}
