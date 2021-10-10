using System.Collections.Generic;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services
{
    public interface IFallbackTextReferenceParser
    {
        List<FallbackTextFunctionReference> Parse(string template);
    }
}
