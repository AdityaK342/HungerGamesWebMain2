using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;

namespace HungerGames
{
    // Comment in exactly one of these.  You must have one with a perceptron and one not
    public class ChooserAditya : LocationChooserTemplateIntermediate<HarePerceptronIntelligenceAditya, LynxIntelligenceAditya>
    //public class ChooserYOURNAME : LocationChooserTemplateIntermediate<HareIntelligenceYOURNAME, LynxPerceptronIntelligenceYOURNAME>
    {
    }
}
