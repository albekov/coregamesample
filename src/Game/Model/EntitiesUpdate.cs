using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Model
{
    public class EntitiesUpdate
    {
        [JsonProperty(PropertyName = "updated")]
        public ICollection<GameEntity> Updated { get; set; }

        [JsonProperty(PropertyName = "removed")]
        public ICollection<string> Removed { get; set; }
    }
}