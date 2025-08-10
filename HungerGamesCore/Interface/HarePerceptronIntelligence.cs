using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using NeuralNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames.Interface
{
    abstract public class HarePerceptronIntelligence : HareIntelligence
    {
        public HarePerceptronIntelligence()
        {

            string filename = FileUtilities.GetMainProjectDirectory() + "Perceptrons/" + PerceptronFilename;
            // Only load if the file exists.
            if (File.Exists(filename))
            {
                Perceptron = Perceptron.ReadFromFile(filename);
            }
            else
            {
                throw new FileNotFoundException($"File {filename} not found!");
            }
        }

        internal Perceptron Perceptron { get; set; }

        abstract protected double[] GetInputs();
        abstract protected string PerceptronFilename { get; }

        override public Turn ChooseAction()
        {
            Perceptron.Reset();
            var inputs = GetInputs();
            Perceptron.AddInputs(inputs);
            Perceptron.Run();

            var accelX = Perceptron.GetOutput(0);
            var accelY = Perceptron.GetOutput(1);

            // Save time by explicitly checking for NaN
            if (UtilityFunctions.IsValid(accelX) && UtilityFunctions.IsValid(accelY))
            {
                return ChangeVelocity(new Vector2D(accelX, accelY));
            }
            else
            {
                return null;
            }

        }
    }
}
