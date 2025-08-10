using Arena;
using HungerGames.Animals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DongUtility;
using System.Drawing;

namespace HungerGames.Interface
{
    abstract public class Intelligence
    {
        internal IntelligentOrganism Organism { get; set; }

        abstract public string Name { get; }
        abstract public Color Color { get; }
        abstract public string BitmapFilename { get; }
        //abstract public string PerceptronFilename { get; }
        abstract public Turn ChooseAction();

        public double ArenaWidth => Organism.Arena.Width;
        public double ArenaHeight => Organism.Arena.Height;
        
        // Utility functions
        static protected Random Random => ArenaEngine.Random;       
    }
}
