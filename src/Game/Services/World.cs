using System;
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
        private static readonly Random R = new Random();

        private readonly List<GameEntity> _entities = new List<GameEntity>();

        public WorldInfo Info { get; set; }

        public void Init()
        {
            Info = new WorldInfo
            {
                XMin = -1000,
                YMin = -1000,
                XMax = 1000,
                YMax = 1000
            };
            InitEntities();
        }

        private void InitEntities()
        {
            for (var i = 0; i < 100; i++)
                _entities.Add(new GameEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "food",
                    Name = $"Entity {i}",
                    X = R.Between(Info.XMin+50, Info.XMax-50),
                    Y = R.Between(Info.YMin + 50, Info.YMax - 50),
                    DX = R.Between(-100, 100, 1),
                    DY = R.Between(-100, 100, 1),
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

                    if (R.NextDouble() < 0.01)
                        SetSpeed(entity, time, R.Between(-100, 100, 1), R.Between(-100, 100, 1));
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