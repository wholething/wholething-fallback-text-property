using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextResolver
    {
        bool CanResolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context);
        IPublishedContent Resolve(FallbackTextFunctionReference reference, FallbackTextResolverContext context);
        void CheckArguments(string[] args, FallbackTextResolverContext context);
    }
}
