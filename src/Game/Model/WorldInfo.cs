using Newtonsoft.Json;

namespace Game.Model
{
    public class WorldInfo
    {
        [JsonProperty(PropertyName = "x0")]
        public double X0 { get; set; }

        [JsonProperty(PropertyName = "y0")]
        public double Y0 { get; set; }

        [JsonProperty(PropertyName = "width")]
        public double Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public double Height { get; set; }
    }
}