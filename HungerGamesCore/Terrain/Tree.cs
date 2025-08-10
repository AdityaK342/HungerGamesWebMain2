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
    public class Tree : Obstacle
    {

        private const string name = "Tree";
        private const string filename = "tree.png";

        private const double maxWidth = 1;
        private const double maxHeight = 1;
        private const double minWidth = .5;
        private const double minHeight = .5;

        public Tree(HungerGamesArena arena) :
            base(arena, filename, ArenaEngine.Random.NextDouble(minWidth, maxWidth), ArenaEngine.Random.NextDouble(minHeight, maxHeight))
        { }

        public override string Name => name;
        public override double VelocityReduction => 100;
        public override bool IsPassable(ArenaObject mover) => false;
    }
}
