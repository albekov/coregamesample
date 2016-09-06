using Newtonsoft.Json;

namespace Game.Model
{
    public class GameEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "x")]
        public float X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public float Y { get; set; }

        [JsonProperty(PropertyName = "dx")]
        public float DX { get; set; }

        [JsonProperty(PropertyName = "dy")]
        public float DY { get; set; }

        [JsonIgnore]
        public double Updated { get; set; }

        [JsonIgnore]
        public Point Target { get; set; }
    }
}