using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HungerGames.Turns;
using Arena;
using System.IO;
using HungerGames.Interface;
using DongUtility;
using System.Drawing;
using NeuralNet;

namespace HungerGames.Animals
{
    abstract public class IntelligentOrganism : MovingObject
    {
        private Intelligence intelligence;

        public void SetPerceptron(Perceptron perceptron)
        {
            if (intelligence is HarePerceptronIntelligence hare)
            {
                hare.Perceptron = perceptron;
            }
            else if (intelligence is LynxPerceptronIntelligence lynx)
            {
                lynx.Perceptron = perceptron;
            }
            //else
            //{
            //    throw new InvalidOperationException("Perceptron can only be set for Hare or Lynx with PerceptronIntelligence");
            //}
        }

        public override string Name => intelligence.Name;
        public Color Color => intelligence.Color;

        private const int orgLayer = 2;

        public IntelligentOrganism(HungerGamesArena arena, Intelligence intel, double width, double height) :
            base(0, orgLayer, width, height)
        {
            intelligence = intel;
            intelligence.Organism = this;
            if (arena != null)
            {
                Arena = arena;
                GraphicCode = arena.GetGraphicsCode(intel, width, height);
                SpeciesCode = arena.GetSpeciesCode(intel);
            }
        }

        public int SpeciesCode { get; }

        virtual protected Turn HungerGamesChooseAction()
        {
            try
            {
                return intelligence.ChooseAction();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
