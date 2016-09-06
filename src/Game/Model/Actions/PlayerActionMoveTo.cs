namespace Game.Model.Actions
{
    public class PlayerActionMoveTo : PlayerAction
    {
        public float X { get; set; }
        public float Y { get; set; }

        public PlayerActionMoveTo(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}