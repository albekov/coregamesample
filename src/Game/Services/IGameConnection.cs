using Game.Model;

namespace Game.Services
{
    public interface IGameConnection
    {
        void Start(object info);
        void Stop();
        void Update(GameUpdate update);
    }
}