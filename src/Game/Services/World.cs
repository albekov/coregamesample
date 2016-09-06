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
        private static readonly Random R = new Random();

        private readonly List<GameEntity> _entities = new List<GameEntity>();

        private ConcurrentDictionary<string, Player> _players =
            new ConcurrentDictionary<string, Player>();

        public WorldInfo Info { get; set; }

        public void Init()
        {
            Info = new WorldInfo
            {
                X0 = -1000,
                Y0 = -1000,
                Width = 2000,
                Height = 2000
            };
            InitEntities();
        }

        private void InitEntities()
        {
            for (var i = 0; i < 10; i++)
                _entities.Add(new GameEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "food",
                    Name = $"Entity {i}",
                    X = R.Between(100, 500),
                    Y = R.Between(100, 300),
                    DX = R.Between(-25, 25, 1),
                    DY = R.Between(-25, 25, 1),
                    Updated = 0
                });
        }

        public void UpdateObjects(double prevTime, double time)
        {
            var dt = (float) ((time - prevTime)/1000.0);
            foreach (var entity in _entities)
            {
                entity.X += entity.DX*dt;
                entity.Y += entity.DY*dt;

                if (entity.Type == "food")
                {
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
                        entity.DX = R.Between(-25, 25, 1);
                        entity.DY = R.Between(-25, 25, 1);
                        entity.Updated = time;
                    }
                }

                if (entity.Target != null)
                {
                    if (entity.Target.IsNear(entity.X, entity.Y, 20))
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
                }

                if (time - entity.Updated >= 5000)
                    entity.Updated = time;
            }
        }

        private void SetSpeed(GameEntity entity, double time, float? dx, float? dy)
        {
            if (dx.HasValue)
            {
                if (Math.Abs(dx.Value - entity.DX) > 0.1)
                {
                    entity.DX = dx.Value;
                    entity.Updated = time;
                }
            }
            if (dy.HasValue)
            {
                if (Math.Abs(dy.Value - entity.DY) > 0.1)
                {
                    entity.DY = dy.Value;
                    entity.Updated = time;
                }
            }
        }

        public GameEntity ConnectPlayer(Player player)
        {
            if (_entities.Any(e => e.Id == player.Id))
                throw new ArgumentException();

            var playerEntity = CreatePlayerEntity(player);
            _entities.Add(playerEntity);

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
            if (_entities.RemoveAll(e => e.Id == player.Id) != 1) return false;

            return true;
        }

        public List<GameEntity> GetWorldUpdate(double updateTime)
        {
            var updated = _entities.Where(e => e.Updated >= updateTime).ToList();
            return updated;
        }

        public void MovePlayer(Player player, float x, float y)
        {
            var entity = _entities.FirstOrDefault(e => e.Id == player.Id);

            entity.Target = new Point(x, y);
        }
    }
}