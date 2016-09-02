using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class PlayersHandler
    {
        private readonly ConnectionHandler _connectionHandler;

        private readonly ConcurrentDictionary<string, Player> _players =
            new ConcurrentDictionary<string, Player>();

        private Func<IList<string>, dynamic> _getChannel;

        public PlayersHandler(ConnectionHandler connectionHandler)
        {
            _connectionHandler = connectionHandler;

            connectionHandler.ConnectionChanged += ConnectionHandlerOnConnectionChanged;
        }

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        private void ConnectionHandlerOnConnectionChanged(object sender, ConnectionChangedEventArgs args)
        {
            switch (args.Type)
            {
                case ConnectionChangedType.Opened:
                    break;
                case ConnectionChangedType.Closed:
                    DisconnectPlayer(args.ConnectionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<Player> CreatePlayer(string connectionId)
        {
            var user = await _connectionHandler.GetUser(connectionId);

            if (user.CurrentPlayer == null)
                user.CurrentPlayer = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = user.Username
                };

            ConnectPlayer(connectionId, user.CurrentPlayer);

            return user.CurrentPlayer;
        }

        public ICollection<Player> GetConnectedPlayers()
        {
            return _players.Values;
        }

        private void ConnectPlayer(string connectionId, Player player)
        {
            _players[connectionId] = player;
            OnPlayerChanged(PlayerChangeType.Connected, player);
        }

        private Player DisconnectPlayer(string connectionId)
        {
            Player player;
            _players.TryRemove(connectionId, out player);
            OnPlayerChanged(PlayerChangeType.Disconnected, player);
            return player;
        }

        public dynamic GetPlayerChannel(string playerId)
        {
            var connectionIds = _players.Where(g => g.Value.Id == playerId).Select(g => g.Key).ToList();
            var channel = _getChannel(connectionIds);
            return channel;
        }

        public void SetChannel(Func<IList<string>, dynamic> getChannel)
        {
            _getChannel = getChannel;
        }

        private void OnPlayerChanged(PlayerChangeType type, Player player)
        {
            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs(type, player));
        }
    }
}