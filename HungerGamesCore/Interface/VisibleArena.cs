using Arena;
using DongUtility;
using Geometry.Geometry2D;
using HungerGames;
using HungerGamesCore.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGamesCore.Interface
{
    public class VisibleArena
    {
        private ArenaEngine arena;

        public VisibleArena(ArenaEngine arena)
        {
            this.arena = arena;
        }

        public bool IsBattleRoyale => ((HungerGamesArena)arena).IsBattleRoyale;

        public bool TestPoint(Point point)
        {
            return arena.IsValidLocation(point);
        }

        public bool TestArea(Shape2D area)
        {
            return arena.IsValidLocation(area);
        }

        public IEnumerable<VisibleObstacle> GetObstacles<T>() where T : Obstacle
        {
            foreach (var obstacle in arena.GetObjectsOfType<T>())
                yield return new VisibleObstacle(obstacle);
        }

        public IEnumerable<VisibleObstacle> GetObstacles()
        {
            return GetObstacles<Obstacle>();
        }

        public double Width => arena.Width;
        public double Height => arena.Height;
    }
}
