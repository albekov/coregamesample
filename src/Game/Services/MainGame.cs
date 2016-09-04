using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Game.Services
{
    [UsedImplicitly]
    public class MainGame : IDisposable
    {
        private const double TickTime = 20;

        private static readonly Random R = new Random();

        private readonly List<GameEntity> _entities = new List<GameEntity>();
        private readonly ILogger<MainGame> _logger;

        private readonly PlayersHandler _playersHandler;

        private readonly ConcurrentDictionary<string, double> _updates =
            new ConcurrentDictionary<string, double>();

        private readonly World _world;

        public MainGame(PlayersHandler playersHandler,
            ILogger<MainGame> logger)
        {
            _playersHandler = playersHandler;
            _logger = logger;

            InitEntities();

            _world = new World();
            _world.Init();

            _playersHandler.PlayerChanged += PlayersHandlerOnPlayerChanged;
        }

        public void Dispose()
        {
            _playersHandler.PlayerChanged -= PlayersHandlerOnPlayerChanged;
        }

        private void InitEntities()
        {
            for (var i = 0; i < 10; i++)
                _entities.Add(new GameEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "food",
                    Name = $"Entity {i}",
                    X = GetRandom(100, 500),
                    Y = GetRandom(100, 300),
                    DX = GetRandom(-100, 100, 1),
                    DY = GetRandom(-100, 100, 1),
                    Updated = 0
                });
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

                UpdateObjects(prevTime, lastTime);
                SendUpdates(prevTime, lastTime);

                var workTime = timer.ElapsedMilliseconds - lastTime;
            }
        }

        private void UpdateObjects(double prevTime, double time)
        {
            var dt = (float) ((time - prevTime)/1000.0);
            foreach (var entity in _entities)
            {
                entity.X += entity.DX*dt;
                entity.Y += entity.DY*dt;

                if ((entity.X < 6) || (entity.X > 594))
                {
                    entity.DX = -entity.DX;
                    entity.X += entity.DX*dt;
                    entity.Updated = time;
                }
                if ((entity.Y < 6) || (entity.Y > 394))
                {
                    entity.DY = -entity.DY;
                    entity.Y += entity.DY*dt;
                    entity.Updated = time;
                }

                if (R.NextDouble() < 0.001)
                {
                    entity.DX = GetRandom(-100, 100, 1);
                    entity.DY = GetRandom(-100, 100, 1);
                    entity.Updated = time;
                }
            }
        }

        private static float GetRandom(double from, double to, int decimals = 0)
        {
            return (float) Math.Round(R.NextDouble()*(to - from) + from, decimals);
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

            var updated = _entities.Where(e => e.Updated >= updateTime).ToList();
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

    public class GameUpdate
    {
        [JsonProperty(PropertyName = "entities")]
        public EntitiesUpdate Entities { get; set; }
    }

    public class EntitiesUpdate
    {
        [JsonProperty(PropertyName = "updated")]
        public ICollection<GameEntity> Updated { get; set; }
    }

    public class GameEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "x")]
        public float X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public float Y { get; set; }

        [JsonProperty(PropertyName = "dx")]
        public float DX { get; set; }

        [JsonProperty(PropertyName = "dy")]
        public float DY { get; set; }

        [JsonIgnore]
        public double Updated { get; set; }
    }
}