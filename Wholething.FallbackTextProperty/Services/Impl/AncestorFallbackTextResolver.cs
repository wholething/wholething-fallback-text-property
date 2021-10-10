using System;
using Umbraco.Core.Models.PublishedContent;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class ParentFallbackTextResolver : FallbackTextResolver
    {
        protected override string FunctionName => "parent";
        public override void CheckArguments(string[] args)
        {
            if (args.Length > 0)
            {
                throw new ArgumentException("Did not expect any arguments");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            return context.Owner.Parent;
        }
    }
}
