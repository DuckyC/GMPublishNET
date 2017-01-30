using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace gmpublish.GMADZip
{
    class AddonJSON : DescriptionJSON
    {
        [JsonProperty("ignore")]
        public List<string> Ignores { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("workshopid")]
        public ulong WorkshopID { get; set; }

        [JsonProperty("default_changelog")]
        public string DefaultChangelog { get; set; }

        public string BuildDescription()
        {
            DescriptionJSON tree = new DescriptionJSON();
            tree.Description = this.Description;

            // Load the addon type
            if (this.Type.ToLowerInvariant() == String.Empty || this.Type.ToLowerInvariant() == null)
                throw new Exception("type is empty!");
            else
            {
                if (!GMADZip.Tags.TypeExists(this.Type.ToLowerInvariant()))
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

                    if (!GMADZip.Tags.TagExists(tag.ToLowerInvariant()))
                        throw new Exception("tag isn't a supported word!");
                    else
                        tree.Tags.Add(tag.ToLowerInvariant());
                }
            }

            return JsonConvert.SerializeObject(tree, Formatting.Indented).Replace("\\u000d", "").Replace("\\u0009", "\\t").Replace("\\u000a", "\\n");
        }

    }
}
