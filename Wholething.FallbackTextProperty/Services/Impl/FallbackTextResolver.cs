using System;
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
        protected readonly IFallbackTextLoggerService Logger;

        protected FallbackTextResolver(IFallbackTextLoggerService logger)
        {
            Logger = logger;
        }

        protected abstract string FunctionName { get; }
        protected abstract bool RequireContent { get; }

        public bool CanResolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context)
        {
            if (reference.Function != FunctionName)
            {
                return false;
            }
            try
            {
                CheckArguments(reference.Args, context);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Fallback text resolver couldn't be used (see exception detail)");
                return false;
            }
            return true;
        }

        public virtual void CheckArguments(string[] args, FallbackTextResolverContext context)
        {
            if (RequireContent && context.Content == null)
            {
                throw new ArgumentException("Expected content, not an element type (block or element)");
            }
        }

        public IPublishedContent Resolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context)
        {
            return Resolve(reference.Args, context);
        }

        protected abstract IPublishedContent Resolve(string[] args, FallbackTextResolverContext context);
    }
}
