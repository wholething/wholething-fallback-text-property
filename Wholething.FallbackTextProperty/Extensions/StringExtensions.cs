using System.Text.RegularExpressions;

namespace Wholething.FallbackTextProperty.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Removes characters that are not valid in a text fallback key at template rendering
        /// </summary>
        public static string StripNonKeyChars(this string value)
        {
            return Regex.Replace(value, "[^a-zA-Z0-9:-]", "-");
        }
    }
}
