using Newtonsoft.Json;

namespace Game.Model
{
    public class GameUpdate
    {
        [JsonProperty(PropertyName = "entities")]
        public EntitiesUpdate Entities { get; set; }
    }
}