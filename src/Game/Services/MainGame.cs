using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Game.Services
{
    [UsedImplicitly]
    public class MainGame
    {
        private const double TickTime = 100;
        private readonly ILogger<MainGame> _logger;

        private readonly PlayersHandler _playersHandler;

        public MainGame(PlayersHandler playersHandler,
            ILogger<MainGame> logger)
        {
            _playersHandler = playersHandler;
            _logger = logger;
        }

        public void Start(CancellationToken stop)
        {
            Init(stop);
            GameLoop(stop);
            Finish(stop);
        }

        private void GameLoop(CancellationToken stop)
        {
            var timer = Stopwatch.StartNew();
            double lastTime = 0;
            while (!stop.IsCancellationRequested)
            {
                SleepUntil(timer, lastTime + TickTime);

                var prevTime = lastTime;
                lastTime = timer.ElapsedMilliseconds;

                UpdateObjects(prevTime, lastTime);
                SendUpdates(prevTime, lastTime);

                var workTime = timer.ElapsedMilliseconds - lastTime;
            }
        }

        private static void SleepUntil(Stopwatch timer, double time)
        {
            SpinWait.SpinUntil(() => timer.ElapsedMilliseconds >= time);
        }

        private void UpdateObjects(double time, double prevTime)
        {
        }

        private void SendUpdates(double time, double prevTime)
        {
        }

        private void Init(CancellationToken stop)
        {
        }

        private void Finish(CancellationToken stop)
        {
        }
    }
}