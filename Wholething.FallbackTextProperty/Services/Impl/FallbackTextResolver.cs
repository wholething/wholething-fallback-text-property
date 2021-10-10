using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public abstract class FallbackTextResolver : IFallbackTextResolver
    {
        protected abstract string FunctionName { get; }

        public bool CanResolve(FallbackTextFunctionReference reference)
        {
            CheckArguments(reference.Args);
            return reference.Function == FunctionName;
        }

        public abstract void CheckArguments(string[] args);

        public IPublishedContent Resolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context)
        {
            return Resolve(reference.Args, context);
        }

        protected abstract IPublishedContent Resolve(string[] args, FallbackTextResolverContext context);
    }
}
