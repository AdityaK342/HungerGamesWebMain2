using Arena;
using HungerGames.Animals;
using HungerGames.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DongUtility;
using System.Collections.Concurrent;
using HungerGamesCore.Interface;
using NeuralNet;
using Geometry.Geometry2D;

namespace HungerGames
{
    public class GameMaster
    {
        private HungerGamesArena arena;
        public List<LocationChooser> Choosers { get; } = new List<LocationChooser>();
        private readonly VisibleArena va;
        private List<Tuple<Point, Animal>> allAnimals = [];

        public GameMaster(HungerGamesArena arena)
        {
            this.arena = arena;
            va = new VisibleArena(arena);
        }

        public void AddChooser(LocationChooser lc)
        {
            Choosers.Add(lc);
        }

        public void AddAllAnimals(int nHare, int nLynx, Perceptron harePerceptron = null, Perceptron lynxPerceptron = null)
        {
            foreach (var chooser in Choosers)
            {
                for (int ihare = 0; ihare < nHare; ++ihare)
                {
                    DoChoice(chooser, true, ihare);
                    if (harePerceptron != null)
                    {
                        allAnimals.Last().Item2.SetPerceptron(harePerceptron);
                    }
                }
                for (int ilynx = 0; ilynx < nLynx; ++ilynx)
                {
                    DoChoice(chooser, false, ilynx);
                    if (lynxPerceptron != null)
                    {
                        allAnimals.Last().Item2.SetPerceptron(lynxPerceptron);
                    }
                }
            }

            foreach (var entry in allAnimals)
            {
                Point position = entry.Item1;
                var animal = entry.Item2;
                var rectangle = new AlignedRectangle(position, animal.Shape.Range.Width, animal.Shape.Range.Height);
                if (!arena.IsValidLocation(rectangle))
                {
                    position = MoveAnimalToRandomLocation(arena, entry.Item2);
                }

                arena.AddObject(animal, position);
            }

            // let arena know which lynx goes with which hare, for scoring
            foreach (var chooser in Choosers)
            {
                int harecode = -1, lynxcode = -1;
                var hares = arena.GetObjectsOfType<Hare>();
                foreach (var hare in hares)
                {
                    if (hare.Name == chooser.GetName(true))
                    {
                        harecode = hare.SpeciesCode;
                        break;
                    }
                }
                var lynxes = arena.GetObjectsOfType<Lynx>();
                foreach (var lynx in lynxes)
                {
                    if (lynx.Name == chooser.GetName(false))
                    {
                        lynxcode = lynx.SpeciesCode;
                        break;
                    }
                }
                if (harecode == -1 || lynxcode == -1)
                    throw new Exception("Something has gone seriously wrong.");

                arena.HareToLynxMapping.Add(harecode, lynxcode);
            }
        }

        private Point MoveAnimalToRandomLocation(HungerGamesArena arena, Animal animal)
        {
            Shape2D rect;
            do
            {
                var position = new Point(ArenaEngine.Random.NextDouble(animal.Shape.Range.Width,
                    arena.Width - animal.Shape.Range.Width),
                    ArenaEngine.Random.NextDouble(animal.Shape.Range.Height, arena.Height - animal.Shape.Range.Height));
                rect = new AlignedRectangle(position, animal.Shape.Range.Width, animal.Shape.Range.Height);
            } while (!arena.IsValidLocation(rect));
            return rect.Center;
        }

        private void DoChoice(LocationChooser lc, bool hare, int counter)
        {
            var location = lc.ChooseLocation(va, hare, counter);

            Animal animal;

            animal = lc.MakeOrganism(arena, hare);

            // Store in a list first so that they can't see each others' choices
            allAnimals.Add(new Tuple<Point, Animal>(location, animal));
        }
    }
}
