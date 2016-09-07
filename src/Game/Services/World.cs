using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Game.Model;
using Game.Utils;
using JetBrains.Annotations;

namespace Game.Services
{
    [UsedImplicitly]
    public class World
    {
        private const double MinTimeToUpdate = 100;
        private const float EntityMaxSpeed = 25;
        private static readonly Random R = new Random();

        private readonly Dictionary<string, GameEntity> _entities = new Dictionary<string, GameEntity>();
        private ConcurrentBag<GameEntity> _toAdd = new ConcurrentBag<GameEntity>();
        private ConcurrentBag<string> _toRemove = new ConcurrentBag<string>();

        private readonly ConcurrentDictionary<string, HashSet<string>> _visible =
            new ConcurrentDictionary<string, HashSet<string>>();

        public WorldInfo Info { get; set; }

        public void Init()
        {
            Info = new WorldInfo
            {
                XMin = -2000,
                YMin = -2000,
                XMax = 2000,
                YMax = 2000
            };
            InitEntities();
        }

        private void InitEntities()
        {
            for (var i = 0; i < 1000; i++)
                _toAdd.Add(new GameEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "food",
                    Name = $"Entity {i}",
                    X = R.Between(Info.XMin + 50, Info.XMax - 50),
                    Y = R.Between(Info.YMin + 50, Info.YMax - 50),
                    DX = R.Between(-EntityMaxSpeed, EntityMaxSpeed, 1),
                    DY = R.Between(-EntityMaxSpeed, EntityMaxSpeed, 1),
                    Updated = 0
                });
        }

        public void UpdateObjects(double prevTime, double time)
        {
            AddRemoveEntities();

            var dt = (float) ((time - prevTime)/1000.0);
            foreach (var entity in _entities.Values)
            {
                entity.X += entity.DX*dt;
                entity.Y += entity.DY*dt;

                if (entity.Type == "food")
                {
                    if ((entity.X < Info.XMin + 10) || (entity.X > Info.XMax - 10))
                    {
                        SetSpeed(entity, time, -entity.DX);
                        entity.X += entity.DX*dt;
                    }
                    if ((entity.Y < Info.YMin + 10) || (entity.Y > Info.YMax - 10))
                    {
                        SetSpeed(entity, time, dy: -entity.DY);
                        entity.Y += entity.DY*dt;
                    }

                    if (R.NextDouble() < 0.001)
                        SetSpeed(entity, time, R.Between(-EntityMaxSpeed, EntityMaxSpeed, 1), R.Between(-EntityMaxSpeed, EntityMaxSpeed, 1));
                }

                if (entity.Target != null)
                    if (entity.Target.IsNear(entity.X, entity.Y, 5))
                    {
                        SetSpeed(entity, time, 0, 0);
                        entity.Target = null;
                    }
                    else
                    {
                        var dx = entity.Target.X - entity.X;
                        var dy = entity.Target.Y - entity.Y;

                        var d = (float) Math.Sqrt(dx*dx + dy*dy);
                        dx = 100*dx/d;
                        dy = 100*dy/d;

                        SetSpeed(entity, time, dx, dy);
                    }

                if (time - entity.Updated >= 5000)
                    entity.Updated = time;
            }
        }

        private void AddRemoveEntities()
        {
            if (_toRemove.Any())
            {
                var toRemove = _toRemove.ToList();
                _toRemove = new ConcurrentBag<string>();
                foreach (var id in toRemove)
                    _entities.Remove(id);
            }

            if (_toAdd.Any())
            {
                var toAdd = _toAdd.ToList();
                _toAdd = new ConcurrentBag<GameEntity>();
                foreach (var entity in toAdd)
                    _entities.Add(entity.Id, entity);
            }
        }

        private void SetSpeed(GameEntity entity, double time, float? dx = null, float? dy = null)
        {
            if (dx.HasValue)
                if (Math.Abs(dx.Value - entity.DX) > 0.1)
                {
                    entity.DX = dx.Value;
                    UpdateTime(entity, time);
                }
            if (dy.HasValue)
                if (Math.Abs(dy.Value - entity.DY) > 0.1)
                {
                    entity.DY = dy.Value;
                    UpdateTime(entity, time);
                }
        }

        private static void UpdateTime(GameEntity entity, double time)
        {
            if (time - entity.Updated >= MinTimeToUpdate)
                entity.Updated = time;
        }

        public GameEntity ConnectPlayer(Player player)
        {
            if (_entities.ContainsKey(player.Id))
                throw new ArgumentException();

            var playerEntity = CreatePlayerEntity(player);
            _toAdd.Add(playerEntity);

            return playerEntity;
        }

        private GameEntity CreatePlayerEntity(Player player)
        {
            return new GameEntity
            {
                Id = player.Id,
                Name = player.Name,
                Type = "player",
                X = player.X,
                Y = player.Y
            };
        }

        public bool DisconnectPlayer(Player player)
        {
            if (player == null) return false;
            _toRemove.Add(player.Id);

            return true;
        }

        public EntitiesUpdate GetWorldUpdate(string playerId, double updateTime)
        {
            if (_toAdd.Any() || _toRemove.Any())
                return null;

            var player = _entities[playerId];

            var visible = FindVisible(player).ToDictionary(e => e.Id);

            var prevVisible = _visible.GetOrAdd(playerId, id => new HashSet<string>());

            var removed = prevVisible.Except(visible.Keys).ToList();
            var added = visible.Keys.Except(prevVisible).ToList();
            var notChanged = visible.Keys.Intersect(prevVisible);

            if (removed.Any() || added.Any())
                _visible[playerId] = new HashSet<string>(visible.Keys);

            var updated = notChanged
                .Select(id => _entities[id]).Where(e => e.Updated > updateTime)
                .Concat(added.Select(id => _entities[id]))
                .ToList();

            if (!updated.Any() && !removed.Any())
                return null;

            var update = new EntitiesUpdate
            {
                Updated = updated,
                Removed = removed
            };

            return update;
        }

        private IEnumerable<GameEntity> FindVisible(GameEntity player)
        {
            return _entities.Values.Where(e => CoordExtensions.IsNear(player.X, player.Y, e.X, e.Y, 300));
        }

        public void MovePlayer(Player player, float x, float y)
        {
            var entity = _entities.Values.FirstOrDefault(e => e.Id == player.Id);

            entity.Target = new Point(x, y);
        }
    }
}