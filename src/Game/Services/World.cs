using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Game.Model;
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
                    X = GetRandom(100, 500),
                    Y = GetRandom(100, 300),
                    DX = GetRandom(-100, 100, 1),
                    DY = GetRandom(-100, 100, 1),
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

                if (time - entity.Updated >= 5000)
                    entity.Updated = time;
            }
        }

        private static float GetRandom(double from, double to, int decimals = 0)
        {
            return (float) Math.Round(R.NextDouble()*(to - from) + from, decimals);
        }

        public bool ConnectPlayer(string player)
        {
            return true;
        }

        public bool DisconnectPlayer(string player)
        {
            return true;
        }

        public List<GameEntity> GetWorldUpdate(double updateTime)
        {
            var updated = _entities.Where(e => e.Updated >= updateTime).ToList();
            return updated;
        }
    }
}