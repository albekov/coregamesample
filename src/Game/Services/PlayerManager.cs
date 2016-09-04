using System;
using System.Collections.Concurrent;
using Game.Model;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class PlayerManager
    {
        private readonly ConcurrentDictionary<string, Player> _playersByUsers =
            new ConcurrentDictionary<string, Player>();

        public Player LoadPlayer(GameUser user)
        {
            var player = _playersByUsers.GetOrAdd(user.Id, id => new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = user.Username,
                UserId = user.Id
            });
            return player;
        }
    }
}