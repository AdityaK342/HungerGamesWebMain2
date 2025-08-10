using Arena;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGames.Turns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DongUtility;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;
using Geometry.Geometry2D;

namespace HungerGames.Interface
{
    abstract public class AnimalIntelligence : Intelligence
    {
        private protected Animal Animal => (Animal)Organism;

        protected VisibleAnimal VisibleAnimal => new VisibleAnimal(Animal);

        public bool IsSameSpecies(VisibleAnimal ani)
        {
            return ani.Species == Animal.SpeciesCode;
        }

        public bool IsBattleRoyale => ((HungerGamesArena)(Animal.Arena)).IsBattleRoyale;

        protected Point Position => Animal.Position;
        protected Shape2D Size => Animal.Shape;
        protected Vector2D Velocity => Animal.Velocity;
        protected double Stamina => Animal.Stamina;

        protected IEnumerable<VisibleAnimal> GetOtherAnimals<T>() where T : Animal
        {
            foreach (var ani in Animal.GetVisibleObjects<T>())
            {
                if (ani != Animal)
                    yield return ani.VisibleAnimal;
            }
        }

        protected IEnumerable<VisibleObstacle> GetObstacles<T>() where T : Obstacle
        {
            return from obstacles in Animal.GetVisibleObjects<T>()
                   select obstacles.VisibleObstacle;
        }

        protected IEnumerable<VisibleAnimal> GetAnimalsSorted()
        {
            var response = new List<VisibleAnimal>();
            foreach (var ani in Animal.GetVisibleObjectsSorted<Animal>())
            {
                if (!ani.Dead && ani != Animal)
                {
                    response.Add(new VisibleAnimal(ani));
                }
            }
            return response;
        }

        protected IEnumerable<VisibleObstacle> GetObstaclesSorted()
        {
            var response = new List<VisibleObstacle>();
            foreach (var ani in Animal.GetVisibleObjectsSorted<Obstacle>())
            {
                response.Add(new VisibleObstacle(ani));
            }
            return response;
        }

        protected VisibleAnimal GetClosest(bool lynx)
        {
            foreach (var animal in GetAnimalsSorted())
            {
                if (animal.IsLynx == lynx)
                {
                    return animal;
                }
            }
            return null;
        }

        protected Turn ChangeVelocity(Vector2D deltaV)
        {
            return new ChangeVelocity(Animal, deltaV);
        }
    }
}
