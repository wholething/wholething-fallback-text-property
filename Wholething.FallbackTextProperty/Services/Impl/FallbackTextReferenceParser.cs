using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wholething.FallbackTextProperty.Services.Models;

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class FallbackTextReferenceParser : IFallbackTextReferenceParser
    {
        private const string FunctionReferencePattern = @"{{(([a-zA-Z]+)(\(([\w, ]+)\))?):\w+}}";

        public List<FallbackTextFunctionReference> Parse(string template)
        {
            var regex = new Regex(FunctionReferencePattern);
            var matches = regex.Matches(template);

            var references = new List<FallbackTextFunctionReference>();

            foreach (Match match in matches)
            {
                var args = match.Groups.Count < 5 ?
                    new string[0] :
                    match.Groups[4].Value
                        .Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                references.Add(new FallbackTextFunctionReference(
                    match.Groups[2].Value,
                    args,
                    match.Groups[1].Value
                ));
            }

            return references.ToList();
        }
    }
}
