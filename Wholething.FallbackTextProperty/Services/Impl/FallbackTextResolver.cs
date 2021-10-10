using Umbraco.Core.Models.PublishedContent;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public abstract class FallbackTextResolver : IFallbackTextResolver
    {
        protected abstract string FunctionName { get; }

        public bool CanResolve(string function, string[] args)
        {
            CheckArguments(args);
            return function == FunctionName;
        }

        public abstract void CheckArguments(string[] args);

        public IPublishedContent Resolve(string function, string[] args, FallbackTextResolverContext context)
        {
            return Resolve(args, context);
        }

        protected abstract IPublishedContent Resolve(string[] args, FallbackTextResolverContext context);
    }
}
