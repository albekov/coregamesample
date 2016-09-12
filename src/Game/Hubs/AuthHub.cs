using System.Threading.Tasks;
using Game.Services;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Game.Hubs
{
    [UsedImplicitly]
    public class AuthHub : Hub
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly ILogger<AuthHub> _log;

        public AuthHub(
            ILogger<AuthHub> log,
            ConnectionHandler connectionHandler)
        {
            _log = log;
            _connectionHandler = connectionHandler;
        }

        public override async Task OnConnected()
        {
            _log.LogDebug("OnConnected");

            await _connectionHandler.OpenConnection(Context.ConnectionId);
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            _log.LogDebug("OnDisconnected");

            await _connectionHandler.CloseConnection(Context.ConnectionId);
        }

        public override async Task OnReconnected()
        {
            _log.LogDebug("OnReconnected");

            await Task.FromResult((object) null);
        }

        [UsedImplicitly]
        public async Task<string> Init(string authId)
        {
            return await _connectionHandler.AttachToSession(Context.ConnectionId, authId);
        }

        [UsedImplicitly]
        public async Task<string> Login(string username, string password)
        {
            return await _connectionHandler.Login(Context.ConnectionId, username, password);
        }

        [UsedImplicitly]
        public async void Logout()
        {
            await _connectionHandler.Logout(Context.ConnectionId);
        }
    }
}