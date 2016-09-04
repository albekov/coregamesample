using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Game.Services
{
    [UsedImplicitly]
    public class MainGame : IDisposable
    {
        private const double TickTime = 20;

        private readonly ILogger<MainGame> _logger;

        private readonly PlayersHandler _playersHandler;

        private readonly ConcurrentDictionary<string, double> _updates =
            new ConcurrentDictionary<string, double>();

        private readonly World _world;

        public MainGame(
            World world,
            PlayersHandler playersHandler,
            ILogger<MainGame> logger)
        {
            _world = world;
            _playersHandler = playersHandler;
            _logger = logger;

            _playersHandler.PlayerChanged += PlayersHandlerOnPlayerChanged;
        }

        public void Dispose()
        {
            _playersHandler.PlayerChanged -= PlayersHandlerOnPlayerChanged;
        }

        private void PlayersHandlerOnPlayerChanged(object sender, PlayerChangedEventArgs args)
        {
            _logger.LogInformation($"PlayerChanged {args.Type} {args.PlayerId}");
            switch (args.Type)
            {
                case PlayerChangeType.Connected:
                    _world.ConnectPlayer(args.PlayerId);
                    var worldInfo = _world.Info;
                    _playersHandler.GetChannel(args.ConnectionId).start(worldInfo);
                    break;
                case PlayerChangeType.Disconnected:
                    _playersHandler.GetChannel(args.ConnectionId).stop();
                    _world.DisconnectPlayer(args.PlayerId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void StartGame(CancellationToken stop)
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

                _world.UpdateObjects(prevTime, lastTime);
                SendUpdates(prevTime, lastTime);

                var workTime = timer.ElapsedMilliseconds - lastTime;
            }
        }

        private void SendUpdates(double prevTime, double time)
        {
            var playerIds = _playersHandler.GetConnectedPlayers();
            foreach (var playerId in playerIds)
            {
                var connectionsIds = _playersHandler.GetPlayerConnections(playerId);
                foreach (var connectionId in connectionsIds)
                {
                    var update = GetGameUpdate(playerId, connectionId);
                    if (update != null)
                    {
                        var channel = _playersHandler.GetChannel(connectionId);
                        channel.Update(update);
                        SaveGameUpdate(playerId, connectionId, time);
                    }
                }
            }
        }

        private GameUpdate GetGameUpdate(string playerId, string connectionId)
        {
            double updateTime = 0;
            _updates.TryGetValue(connectionId, out updateTime);

            var updated = _world.GetWorldUpdate(updateTime);
            if (!updated.Any())
                return null;

            var update = new GameUpdate
            {
                Entities = new EntitiesUpdate
                {
                    Updated = updated
                }
            };
            return update;
        }

        private void SaveGameUpdate(string playerId, string connectionId, double time)
        {
            _updates[connectionId] = time;
        }

        private static void SleepUntil(Stopwatch timer, double time)
        {
            SpinWait.SpinUntil(() => timer.ElapsedMilliseconds >= time);
        }

        private void Init(CancellationToken stop)
        {
            _world.Init();
        }

        private void Finish(CancellationToken stop)
        {
        }

        public async Task Start(string connectionId)
        {
            await _playersHandler.ConnectPlayer(connectionId);
        }

        public async Task Stop(string connectionId)
        {
            await _playersHandler.DisconnectPlayer(connectionId);
        }
    }
}