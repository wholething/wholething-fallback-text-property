namespace Wholething.FallbackTextProperty.Services.Models
{
    public class FallbackTextFunctionReference
    {
        public string Function { get; set; }
        public string[] Args { get; set; }
        public string Key { get; set; }

        public FallbackTextFunctionReference(string function, string[] args, string key)
        {
            Function = function;
            Args = args;
            Key = key;
        }
    }
}
