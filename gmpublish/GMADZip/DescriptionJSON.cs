using Newtonsoft.Json;
using System.Collections.Generic;

namespace gmpublish.GMADZip
{
    public class DescriptionJSON
    {
        [JsonProperty("description")]
        public string Description;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("tags")]
        public List<string> Tags;
    }
}
