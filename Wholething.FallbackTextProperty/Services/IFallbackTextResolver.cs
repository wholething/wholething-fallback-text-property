using Umbraco.Core.Models.PublishedContent;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextResolver
    {
        bool CanResolve(FallbackTextReference reference);
        IPublishedContent Resolve(FallbackTextReference reference, FallbackTextResolverContext context);
        void CheckArguments(string[] args);
    }
}
