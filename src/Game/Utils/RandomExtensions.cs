using System;

namespace Game.Utils
{
    public static class RandomExtensions
    {
        public static float Between(this Random random, float from, float to, int decimals = 0)
        {
            return (float)Math.Round(random.NextDouble() * (to - from) + from, decimals);
        }
    }
}