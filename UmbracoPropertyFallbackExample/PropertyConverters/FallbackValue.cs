using Newtonsoft.Json;

namespace UmbracoPropertyFallbackExample.PropertyConverters
{
    public class FallbackValue
    {
        [JsonProperty("value")]
        private string _value;
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_value))
                {
                    return Fallback;
                }
                else
                {
                    return _value;
                }
            }
            set
            {
                _value = value;
            }
        }

        public string Fallback { get; set; }
    }
}