using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Game.Hubs
{
    [UsedImplicitly]
    public class GameHub : Hub
    {
        private readonly ILogger<GameHub> _log;
        private readonly PlayersHandler _playersHandler;

        public GameHub(
            ILogger<GameHub> log,
            PlayersHandler playersHandler,
            ConnectionHandler connectionHandler)
        {
            _log = log;
            _playersHandler = playersHandler;

            _playersHandler.SetChannel(GetChannel);
        }

        private dynamic GetChannel(IList<string> connectionIds)
        {
            return Clients.Clients(connectionIds);
        }

        public async Task Start()
        {
            var player = await _playersHandler.CreatePlayer(Context.ConnectionId);

            _log.LogTrace($"Player entered: {player.Name}");
        }
    }
}