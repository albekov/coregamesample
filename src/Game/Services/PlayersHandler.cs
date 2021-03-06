﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Model;
using Game.Model.Actions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Game.Services
{
    [UsedImplicitly]
    public class PlayersHandler : IDisposable
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly ILogger<PlayersHandler> _logger;
        private readonly PlayerManager _playerManager;

        private readonly ConcurrentDictionary<string, string> _playersByConnections =
            new ConcurrentDictionary<string, string>();

        private Func<string, IGameConnection> _getChannel;

        public PlayersHandler(
            ILogger<PlayersHandler> logger,
            ConnectionHandler connectionHandler,
            PlayerManager playerManager)
        {
            _logger = logger;
            _connectionHandler = connectionHandler;
            _playerManager = playerManager;

            _connectionHandler.ConnectionChanged += ConnectionHandlerOnConnectionChanged;
        }

        public void Dispose()
        {
            _connectionHandler.ConnectionChanged -= ConnectionHandlerOnConnectionChanged;
        }

        public event AsyncEventHandler<PlayerChangedEventArgs> PlayerChanged;
        public event AsyncEventHandler<PlayerActionEventArgs> PlayerAction;

        private async Task ConnectionHandlerOnConnectionChanged(object sender, ConnectionChangedEventArgs args)
        {
            switch (args.Type)
            {
                case ConnectionChangedType.Opened:
                    break;
                case ConnectionChangedType.Closed:
                    await DisconnectPlayer(args.ConnectionId);
                    break;
                case ConnectionChangedType.Login:
                    break;
                case ConnectionChangedType.Logout:
                    await DisconnectPlayer(args.ConnectionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<Player> GetPlayer(string connectionId)
        {
            var user = await _connectionHandler.GetUser(connectionId);
            var player = _playerManager.LoadPlayer(user);
            return player;
        }

        public ICollection<string> GetConnectedPlayers()
        {
            return _playersByConnections.Values;
        }

        public async Task ConnectPlayer(string connectionId)
        {
            var player = await GetPlayer(connectionId);

            _playersByConnections[connectionId] = player.Id;
            await OnPlayerChanged(PlayerChangeType.Connected, connectionId, player.Id);
        }

        public async Task<string> DisconnectPlayer(string connectionId)
        {
            string playerId;
            _playersByConnections.TryRemove(connectionId, out playerId);
            await OnPlayerChanged(PlayerChangeType.Disconnected, connectionId, playerId);
            return await Task.FromResult(playerId);
        }

        public List<string> GetPlayerConnections(string playerId)
        {
            var connectionIds = _playersByConnections.Where(p => p.Value == playerId).Select(p => p.Key).ToList();
            return connectionIds;
        }

        public IGameConnection GetChannel(string connectionId)
        {
            return _getChannel(connectionId);
        }

        public void SetChannel(Func<string, IGameConnection> getChannel)
        {
            _getChannel = getChannel;
        }

        private async Task OnPlayerChanged(PlayerChangeType type, string connectionId, string playerId)
        {
            if (PlayerChanged != null)
                await PlayerChanged(this, new PlayerChangedEventArgs(type, connectionId, playerId));
        }

        public async Task HandleAction(string connectionId, PlayerAction action)
        {
            string playerId;
            if (!_playersByConnections.TryGetValue(connectionId, out playerId))
            {
                _logger.LogWarning($"Handle action. Unknown connection {connectionId}.");
                return;
            }

            if (PlayerAction != null)
                await PlayerAction(this, new PlayerActionEventArgs(connectionId, playerId, action));
        }
    }
}