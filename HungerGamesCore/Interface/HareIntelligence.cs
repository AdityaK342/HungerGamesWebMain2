using Arena;
using HungerGames.Animals;
using HungerGames.Turns;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames.Interface
{
    abstract public class HareIntelligence : AnimalIntelligence
    {
        protected bool IsMyLynx(VisibleAnimal ani)
        {
            if (ani.Animal is Lynx lynx)
            {
                return ((HungerGamesArena)(Animal.Arena)).SameStudentWriter((Hare)Organism, lynx);
            }
            else
            {
                return false;
            }
        }
    }
}
