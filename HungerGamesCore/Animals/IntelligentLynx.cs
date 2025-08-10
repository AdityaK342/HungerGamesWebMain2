using Arena;
using HungerGames.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames.Animals
{
    public class IntelligentLynx<T> : Lynx where T : LynxIntelligence, new()
    {
        public IntelligentLynx(HungerGamesArena arena) :
            base(arena, new T())
            {}
    }
}
