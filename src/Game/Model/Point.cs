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

        public bool IsNear(float entityX, float entityY, int r)
        {
            var dx = entityX - X;
            var dy = entityY - Y;
            return dx*dx + dy*dy < r*r;
        }
    }
}