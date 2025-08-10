using Arena;
using HungerGames.Animals;
using HungerGames.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames.Interface
{
    abstract public class LynxIntelligence : AnimalIntelligence
    {
        protected bool IsMyHare(VisibleAnimal ani)
        {
            if (ani.Animal is Hare)
            {
                return ((HungerGamesArena)(Animal.Arena)).SameStudentWriter((Hare)(ani.Animal), (Lynx)(Organism));
            }
            else
            {
                return false;
            }
        }
    }
}
