using Umbraco.Core.Models.PublishedContent;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public abstract class FallbackTextResolver : IFallbackTextResolver
    {
        protected abstract string FunctionName { get; }

        public bool CanResolve(FallbackTextReference reference)
        {
            CheckArguments(reference.Args);
            return reference.Function == FunctionName;
        }

        public abstract void CheckArguments(string[] args);

        public IPublishedContent Resolve(FallbackTextReference reference, FallbackTextResolverContext context)
        {
            return Resolve(reference.Args, context);
        }

        protected abstract IPublishedContent Resolve(string[] args, FallbackTextResolverContext context);
    }
}
