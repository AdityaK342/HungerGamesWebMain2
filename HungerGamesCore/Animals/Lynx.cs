using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arena;
using HungerGames.Interface;
using HungerGamesCore.Interface;

namespace HungerGames.Animals
{
    abstract public class Lynx : Animal
    {
        static private readonly AnimalStats lynxStats = new AnimalStats()
        {
            MaxAcceleration = 4,
            MaxSpeed = 22,
            MaxStamina = 100,
            StaminaPerSecondAtTopSpeed = 10,
            StaminaRestoredPerSecond = 1,
            StepTime = .3,
            WalkingSpeed = 1,
            VisionBase = 100
        };

        private const double lynxWidth = .5*2;
        private const double lynxLength = .5*2;

        public Lynx(HungerGamesArena arena, AnimalIntelligence intel) :
            base(arena, intel, lynxStats, lynxLength, lynxWidth)
        { }

        public int HaresEaten { get; set; }

    }
}
