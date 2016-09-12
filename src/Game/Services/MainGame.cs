using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Game.Model;
using Game.Model.Actions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Game.Services
{
    [UsedImplicitly]
    public class MainGame : IDisposable
    {
        private const double TickTime = 20;

        private readonly ILogger<MainGame> _logger;
        private readonly PlayerManager _playerManager;

        private readonly PlayersHandler _playersHandler;

        private readonly ConcurrentDictionary<string, double> _updates =
            new ConcurrentDictionary<string, double>();

        private readonly World _world;

        public MainGame(
            World world,
            PlayersHandler playersHandler,
            PlayerManager playerManager,
            ILogger<MainGame> logger)
        {
            _world = world;
            _playersHandler = playersHandler;
            _playerManager = playerManager;
            _logger = logger;

            _playersHandler.PlayerChanged += PlayersHandlerOnPlayerChanged;
            _playersHandler.PlayerAction += PlayersHandlerOnPlayerAction;
        }

        public void Dispose()
        {
            _playersHandler.PlayerChanged -= PlayersHandlerOnPlayerChanged;
            _playersHandler.PlayerAction -= PlayersHandlerOnPlayerAction;
        }

        private async Task PlayersHandlerOnPlayerChanged(object sender, PlayerChangedEventArgs args)
        {
            var player = _playerManager.GetPlayer(args.PlayerId);
            _logger.LogInformation($"PlayerChanged {args.Type} {args.PlayerId}: '{player?.Name}'");
            switch (args.Type)
            {
                case PlayerChangeType.Connected:
                    var entity = await _world.ConnectPlayer(player);
                    var worldInfo = _world.Info;
                    _playersHandler.GetChannel(args.ConnectionId).Start(new {world = worldInfo, player = entity});
                    break;
                case PlayerChangeType.Disconnected:
                    await _world.DisconnectPlayer(player);
                    _playersHandler.GetChannel(args.ConnectionId).Stop();
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

            var entitiesUpdate = _world.GetWorldUpdate(playerId, updateTime);
            if (entitiesUpdate == null)
                return null;

            var update = new GameUpdate
            {
                Entities = entitiesUpdate
            };
            return update;
        }

        private void SaveGameUpdate(string playerId, string connectionId, double time)
        {
            _updates[connectionId] = time;
        }

        private async Task PlayersHandlerOnPlayerAction(object sender, PlayerActionEventArgs e)
        {
            var player = _playerManager.GetPlayer(e.PlayerId);

            var action = e.Action;

            var actionMoveTo = action as PlayerActionMoveTo;

            if (actionMoveTo!=null)
            {
                _world.MovePlayer(player, actionMoveTo.X, actionMoveTo.Y);
            }

            await Task.FromResult((object) null);
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
    }
}