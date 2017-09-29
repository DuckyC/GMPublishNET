using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GMPublish.GMAD
{
    public class AddonJSON : DescriptionJSON
    {
        [JsonProperty("ignore")]
        public List<string> Ignores { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("workshopid")]
        public ulong WorkshopID { get; set; }

        [JsonProperty("default_changelog")]
        public string DefaultChangelog { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        public string BuildDescription()
        {
            var tree = new DescriptionJSON
            {
                Description = this.Description
            };

            // Load the addon type
            if (this.Type.ToLowerInvariant() == String.Empty || this.Type.ToLowerInvariant() == null)
                throw new Exception("type is empty!");
            else
            {
                if (!GMAD.Tags.TypeExists(this.Type.ToLowerInvariant()))
                    throw new Exception("type isn't a supported type!");
                else
                    tree.Type = this.Type.ToLowerInvariant();
            }

            // Parse the tags
            tree.Tags = new List<string>();
            if (this.Tags.Count > 2)
                throw new Exception("too many tags - specify 2 only!");
            else
            {
                foreach (string tag in this.Tags)
                {
                    if (tag == String.Empty || tag == null) continue;

                    if (!GMAD.Tags.TagExists(tag.ToLowerInvariant()))
                        throw new Exception("tag isn't a supported word!");
                    else
                        tree.Tags.Add(tag.ToLowerInvariant());
                }
            }

            return JsonConvert.SerializeObject(tree, Formatting.Indented).Replace("\\u000d", "").Replace("\\u0009", "\\t").Replace("\\u000a", "\\n");
        }

    }
}
