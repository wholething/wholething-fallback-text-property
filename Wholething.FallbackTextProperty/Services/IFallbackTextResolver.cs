using Umbraco.Core.Models.PublishedContent;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextResolver
    {
        bool CanResolve(string function, string[] args);
        IPublishedContent Resolve(string function, string[] args, FallbackTextResolverContext context);
        void CheckArguments(string[] args);
    }
}
