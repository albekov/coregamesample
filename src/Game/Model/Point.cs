namespace Game.Model
{
    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public static class CoordExtensions
    {
        public static bool IsNear(float x, float y, float targetX, float targetY, float r)
        {
            var dx = x - targetX;
            var dy = y - targetY;
            return dx * dx + dy * dy < r * r;
        }


        public static bool IsNear(this Point point, float targetX, float targetY, int r)
        {
            return IsNear(point.X, point.Y, targetX, targetY, r);
        }
    }
}