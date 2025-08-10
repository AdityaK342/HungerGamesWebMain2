using HungerGames.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames.Animals
{
    public class IntelligentHare<T> : Hare where T : HareIntelligence, new()
    {
        public IntelligentHare(HungerGamesArena arena) :
            base(arena, new T())
        { }
    }
}
