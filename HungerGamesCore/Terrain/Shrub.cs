using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arena;
using DongUtility;
using HungerGames;

namespace HungerGamesCore.Terrain
{
    public class Shrub : Obstacle
    {

        private const string name = "Shrub";
        private const string filename = "shrub.png";

        private const double maxWidth = 1.5;
        private const double maxHeight = 1.5;
        private const double minWidth = .25;
        private const double minHeight = .25;

        public Shrub(HungerGamesArena arena) :
            base(arena, filename, ArenaEngine.Random.NextDouble(minWidth, maxWidth), ArenaEngine.Random.NextDouble(minHeight, maxHeight))
        {
        }

        public override string Name => name;
        public override double VelocityReduction => 10;
        public override bool IsPassable(ArenaObject mover) => true;
    }
}
