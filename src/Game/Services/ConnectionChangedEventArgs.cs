using System;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class ConnectionChangedEventArgs : EventArgs
    {
        public ConnectionChangedEventArgs(ConnectionChangedType type, string connectionId)
        {
            Type = type;
            ConnectionId = connectionId;
        }

        public ConnectionChangedType Type { get; set; }
        public string ConnectionId { get; set; }
    }
}