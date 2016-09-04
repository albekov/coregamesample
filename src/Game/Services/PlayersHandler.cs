using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class PlayersHandler : IDisposable
    {
        private readonly ConnectionHandler _connectionHandler;

        private readonly ConcurrentDictionary<string, string> _playersByConnections =
            new ConcurrentDictionary<string, string>();

        private Func<string, dynamic> _getChannel;

        public PlayersHandler(ConnectionHandler connectionHandler)
        {
            _connectionHandler = connectionHandler;

            _connectionHandler.ConnectionChanged += ConnectionHandlerOnConnectionChanged;
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
                case ConnectionChangedType.Login:
                    break;
                case ConnectionChangedType.Logout:
                    DisconnectPlayer(args.ConnectionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<Player> GetPlayer(string connectionId)
        {
            var user = await _connectionHandler.GetUser(connectionId);

            if (user.CurrentPlayer == null)
                user.CurrentPlayer = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = user.Username
                };

            return user.CurrentPlayer;
        }

        public ICollection<string> GetConnectedPlayers()
        {
            return _playersByConnections.Values;
        }

        public async Task ConnectPlayer(string connectionId)
        {
            var player = await GetPlayer(connectionId);

            _playersByConnections[connectionId] = player.Id;
            OnPlayerChanged(PlayerChangeType.Connected, connectionId, player.Id);
        }

        public async Task<string> DisconnectPlayer(string connectionId)
        {
            string playerId;
            _playersByConnections.TryRemove(connectionId, out playerId);
            OnPlayerChanged(PlayerChangeType.Disconnected, connectionId, playerId);
            return await Task.FromResult(playerId);
        }

        public List<string> GetPlayerConnections(string playerId)
        {
            var connectionIds = _playersByConnections.Where(p => p.Value == playerId).Select(p => p.Key).ToList();
            return connectionIds;
        }

        public dynamic GetChannel(string connectionId)
        {
            return _getChannel(connectionId);
        }

        public void SetChannel(Func<string, dynamic> getChannel)
        {
            _getChannel = getChannel;
        }

        private void OnPlayerChanged(PlayerChangeType type, string connectionId, string playerId)
        {
            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs(type, connectionId, playerId));
        }

        public void Dispose()
        {
            _connectionHandler.ConnectionChanged -= ConnectionHandlerOnConnectionChanged;
        }
    }
}