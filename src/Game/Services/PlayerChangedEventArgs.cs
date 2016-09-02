using System;

namespace Game.Services
{
    public class PlayerChangedEventArgs : EventArgs
    {
        public PlayerChangedEventArgs(PlayerChangeType type, Player player)
        {
            Type = type;
            Player = player;
        }

        public PlayerChangeType Type { get; set; }
        public Player Player { get; set; }
    }
}