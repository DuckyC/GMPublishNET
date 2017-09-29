using Newtonsoft.Json;
using System.Collections.Generic;

namespace GMPublish.GMAD
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
