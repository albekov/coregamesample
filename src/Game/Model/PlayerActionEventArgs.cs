using System;
using Game.Model.Actions;

namespace Game.Model
{
    public class PlayerActionEventArgs : EventArgs
    {
        public PlayerActionEventArgs(string connectionId, string playerId, PlayerAction action)
        {
            ConnectionId = connectionId;
            PlayerId = playerId;
            Action = action;
        }

        public string ConnectionId { get; set; }
        public string PlayerId { get; set; }
        public PlayerAction Action { get; set; }
    }
}