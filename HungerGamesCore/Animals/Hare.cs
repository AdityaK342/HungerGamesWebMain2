using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arena;
using DongUtility;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;

namespace HungerGames.Animals
{
    abstract public class Hare : Animal
    {
        static private readonly AnimalStats hareStats = new AnimalStats()
        {
            MaxAcceleration = 3,
            MaxSpeed = 12,
            MaxStamina = 75,
            StaminaPerSecondAtTopSpeed = 7,
            StaminaRestoredPerSecond = 2,
            StepTime = .25,
            WalkingSpeed = .5,
            VisionBase = 40
        };

        private const double hareWidth = .2*2;
        private const double hareLength = .2*2;

        public Hare(HungerGamesArena arena, HareIntelligence intel) :
            base(arena, intel, hareStats, hareLength, hareWidth) // So the hare is horizontal
        { }
    }
}
