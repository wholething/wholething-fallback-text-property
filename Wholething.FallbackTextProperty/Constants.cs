using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wholething.FallbackTextProperty
{
    internal class Constants
    {
        internal class Regex
        {
            public const string FunctionReferencePattern = @"{{(([a-zA-Z]+)(\(([\w, -\/]*)\))?):(\w+)}}";
        }
    }
}
