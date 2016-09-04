namespace Game.Model
{
    public class GameUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Player CurrentPlayer { get; set; }
    }
}