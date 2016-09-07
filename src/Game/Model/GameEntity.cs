using System.Collections.Generic;
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

        protected bool Equals(GameEntity other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GameEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id?.GetHashCode() ?? 0)*397) ^ (Type?.GetHashCode() ?? 0);
            }
        }
    }
}