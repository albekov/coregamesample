using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Game.Services
{
    public class World
    {
        private ConcurrentDictionary<string, Player> _players =
            new ConcurrentDictionary<string, Player>();

        public WorldInfo Info { get; set; }

        public void Init()
        {
            Info = new WorldInfo
            {
                X0 = -1000,
                Y0 = -1000,
                Width = 2000,
                Height = 2000
            };
        }

        public bool ConnectPlayer(string player)
        {
            return true;
        }

        public bool DisconnectPlayer(string player)
        {
            return true;
        }
    }
}