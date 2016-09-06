using System;
using System.Collections.Concurrent;
using System.Linq;
using Game.Model;
using Game.Utils;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class PlayerManager
    {
        private static readonly Random R = new Random();

        private readonly ConcurrentDictionary<string, Player> _playersByUsers =
            new ConcurrentDictionary<string, Player>();

        public Player LoadPlayer(GameUser user)
        {
            var player = _playersByUsers.GetOrAdd(user.Id, id => new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = user.Username,
                UserId = user.Id,
                X = R.Between(0, 100),
                Y = R.Between(0, 100)
            });
            return player;
        }

        public Player GetPlayer(string playerId)
        {
            return _playersByUsers.Values.FirstOrDefault(p => p.Id == playerId);
        }
    }
}