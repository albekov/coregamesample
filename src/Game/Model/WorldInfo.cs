using Newtonsoft.Json;

namespace Game.Model
{
    public class WorldInfo
    {
        [JsonProperty(PropertyName = "xmin")]
        public float XMin { get; set; }

        [JsonProperty(PropertyName = "ymin")]
        public float YMin { get; set; }

        [JsonProperty(PropertyName = "xmax")]
        public float XMax { get; set; }

        [JsonProperty(PropertyName = "ymax")]
        public float YMax { get; set; }
    }
}