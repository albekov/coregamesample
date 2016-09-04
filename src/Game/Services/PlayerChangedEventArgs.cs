using System;
using System.Diagnostics;

namespace Game.Services
{
    public class PlayerChangedEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public PlayerChangedEventArgs(PlayerChangeType type, string connectionId, string playerId)
        {
            Type = type;
            ConnectionId = connectionId;
            PlayerId = playerId;
        }

        public PlayerChangeType Type { get; set; }
        public string ConnectionId { get; set; }
        public string PlayerId { get; set; }
    }
}