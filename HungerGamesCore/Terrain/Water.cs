using Arena;
using HungerGames;
using HungerGamesCore.Terrain;
using System;
using System.Collections.Generic;
using System.Text;

namespace HungerGamesCore.Terrain
{
    public class Water : Obstacle
    {
        private const string name = "Water";
        private const string filename = "water.jpg";

        public Water(HungerGamesArena arena, double width, double length) :
            base(arena, filename, width, length)
        {
        }

        public override bool IsDeadly => true;
        public override string Name => name;
        public override double VelocityReduction => 100;
        public override bool IsPassable(ArenaObject mover = null) => false;
    }
}
