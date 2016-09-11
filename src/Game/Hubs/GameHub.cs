using System.Threading.Tasks;
using Game.Model.Actions;
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
            PlayersHandler playersHandler)
        {
            _log = log;
            _playersHandler = playersHandler;

            _playersHandler.SetChannel(GetChannel);
        }

        private dynamic GetChannel(string connectionId)
        {
            return Clients.Client(connectionId);
        }

        public async Task Start()
        {
            await _playersHandler.ConnectPlayer(Context.ConnectionId);
        }

        public async Task Stop()
        {
            await _playersHandler.DisconnectPlayer(Context.ConnectionId);
        }

        public async Task MoveTo(float x, float y)
        {
            await _playersHandler.HandleAction(Context.ConnectionId, new PlayerActionMoveTo(x, y));
        }
    }
}