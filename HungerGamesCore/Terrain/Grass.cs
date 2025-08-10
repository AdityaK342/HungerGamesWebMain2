using Arena;
using HungerGames;
using HungerGames.Animals;
using System.Collections.Generic;

namespace HungerGamesCore.Terrain
{
    public class Grass : Obstacle
    {
        private const string name = "Grass";
        private const string filename = "grass.jpg";

        public Grass(HungerGamesArena arena, double width, double length, double height) :
            base(arena, filename, width, length)
        {
            Height = height;
        }

        public double Height { get; }

        public override string Name => name;

        public override double VelocityReduction => .5 * Height;

        public override bool IsPassable(ArenaObject mover) => true;
    }
}
