using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Model;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class ConnectionHandler
    {
        private readonly ConcurrentDictionary<string, string> _connections =
            new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, GameSession> _sessions =
            new ConcurrentDictionary<string, GameSession>();

        private readonly List<GameUser> _users = new List<GameUser>();

        public ConnectionHandler()
        {
            var user1 = new GameUser {Id = Guid.NewGuid().ToString(), Username = "admin", Password = "123456"};
            _users.Add(user1);

            var session1 = new GameSession {Id = "fake", UserId = user1.Id};
            _sessions[session1.Id] = session1;
        }

        public event AsyncEventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        public async Task OpenConnection(string connectionId)
        {
            _connections[connectionId] = null;
            await OnConnectionChanged(ConnectionChangedType.Opened, connectionId);
        }

        public async Task CloseConnection(string connectionId)
        {
            string sessionId;
            _connections.TryRemove(connectionId, out sessionId);

            await OnConnectionChanged(ConnectionChangedType.Closed, connectionId);

            await Task.FromResult((object) null);
        }

        public async Task<string> AttachToSession(string connectionId, string authId)
        {
#if DEBUG
            if (authId == null)
                authId = "fake";
#endif
            GameSession session;
            if ((authId == null) || !_sessions.TryGetValue(authId, out session))
                return null;

            _connections[connectionId] = session.Id;

            await OnConnectionChanged(ConnectionChangedType.Closed, connectionId);

            return await Task.FromResult(session.Id);
        }

        public async Task<string> Login(string connectionId, string username, string password)
        {
            var user = FindUser(username, password);

            if (user == null)
                return null;

            var session = new GameSession {Id = Guid.NewGuid().ToString(), UserId = user.Id};
            _sessions[session.Id] = session;

            _connections[connectionId] = session.Id;

            await OnConnectionChanged(ConnectionChangedType.Login, connectionId);

            return await Task.FromResult(session.Id);
        }

        private GameUser FindUser(string username, string password)
        {
            GameUser user;

            if ((username == "guest") && (password == "guest"))
            {
                var id = Guid.NewGuid().ToString();
                user = new GameUser
                {
                    Id = id,
                    Username = id.Substring(0, 8)
                };
                _users.Add(user);
            }
            else
            {
                user = _users.FirstOrDefault(u => (u.Username == username) && (u.Password == password));
            }

            return user;
        }

        public async Task<GameUser> GetUser(string connectionId)
        {
            string sessionId;
            _connections.TryGetValue(connectionId, out sessionId);
            if (sessionId == null)
                return null;

            var session = _sessions[sessionId];

            var user = _users.FirstOrDefault(u => u.Id == session.UserId);
            return await Task.FromResult(user);
        }

        public async Task Logout(string connectionId)
        {
            string sessionId;
            if (_connections.TryRemove(connectionId, out sessionId))
            {
                GameSession session;
                _sessions.TryRemove(sessionId, out session);

                await OnConnectionChanged(ConnectionChangedType.Logout, connectionId);
            }
            await Task.FromResult((object) null);
        }

        private async Task OnConnectionChanged(ConnectionChangedType type, string connectionId)
        {
            if (ConnectionChanged != null)
                await ConnectionChanged(this, new ConnectionChangedEventArgs(type, connectionId));
        }
    }
}