using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arena;
using HungerGames.Animals;
using HungerGames.Interface;
using DongUtility;
using HungerGamesCore.Terrain;
using Geometry.Geometry2D;

namespace HungerGames
{
    public class HungerGamesArena : ArenaEngine
    {
        private const string backgroundFilename = "dirt.jpg";
        private readonly double maxTime;

        protected void AddSpeciesCode(Type type, int code)
        {
            if (!speciesCodes.ContainsKey(type))
            {
                speciesCodes.Add(type, code);
            }
        }

        private Dictionary<Type, int> speciesCodes = [];

        private int speciesCodeCounter = 0;
        internal int GetGraphicsCode(Type type, string filename, double width, double height)
        {
            return Registry.AddEntry(new GraphicInfo(filename, width, height));
        }

        internal int GetGraphicsCode(Intelligence intel, double width, double height)
        {
            return GetGraphicsCode(intel.GetType(), intel.BitmapFilename, width, height);
        }

        internal int GetSpeciesCode(Intelligence intel)
        {
            lock (locker)
            {
                var type = intel.GetType();
                if (!speciesCodes.ContainsKey(type))
                {
                    AddSpeciesCode(type, speciesCodeCounter++);
                }
                return speciesCodes[type];
            }
        }

        public bool IsBattleRoyale { get; set; } = false;
        public HungerGamesArena(double width, double height, double maxTime = double.MaxValue, int xDivs = 10, int yDivs = 10) :
            base(width, height, backgroundFilename, xDivs, yDivs)
        {
            Registry.Initialize(@"HungerGames\", @"Graphics\");
            this.maxTime = maxTime;
        }

        static private object locker = new object();

        protected override void UserDefinedBeginningOfTurn()
        {
            base.UserDefinedBeginningOfTurn();

            if (colors == null)
            {
                colors = new Dictionary<string, System.Drawing.Color>();
                foreach (var animal in GetObjectsOfType<Animal>())
                {
                    if (!colors.ContainsKey(animal.Name))
                    {
                        colors.Add(animal.Name, animal.Color);
                    }
                }
            }
        }

        private Dictionary<string, System.Drawing.Color> colors = null;

        public void AddAnimalsPossible<T>(int num) where T : Animals.Animal, new()
        {
            var animalForChecking = new T();
            for (int i = 0; i < num;)
            {
                double xCoord = Random.NextDouble(0, Width);
                double yCoord = Random.NextDouble(0, Height);
                var location = new Point(xCoord, yCoord);
                if (IsOccupied(location, animalForChecking))
                {
                    var ani = new T();
                    AddObject(ani, location);
                    ++i;
                }
            }
        }

        public IEnumerable<Animal> GetOtherAnimals(Animal ani)
        {
            foreach (var animal in GetObjectsOfType<Animal>())
            {
                if (ani != animal)
                    yield return animal;
            }
        }

        private const double treesPerSquareMeter = .05;
        private const double shrubsPerSquareMeter = .025;
        private const double areaOfMaxHeight = 10;
        private const double maxGrassHeight = .5;
        private const int nGrassTries = 5;
        private const int nWaterTries = 5;
        private const double maxWaterSide = 5;

        public override void Initialize()
        {
            for (int i = 0; i < nWaterTries; ++i)
            {
                PlaceWater();
            }
            PlacePlants<Tree>(treesPerSquareMeter);
            PlacePlants<Shrub>(shrubsPerSquareMeter);
            for (int i = 0; i < nGrassTries; ++i)
            {
                PlaceGrass(new Point(Random.NextDouble(0, Width), Random.NextDouble(0, Height)));
            }
        }

        private void PlaceWater()
        {
            AlignedRectangle rectangle, testRectangle;
            do
            {
                var position = new Point(Random.NextDouble(0, Width), Random.NextDouble(0, Height));
                rectangle = new AlignedRectangle(position, Random.NextDouble(0, maxWaterSide),
                    Random.NextDouble(0, maxWaterSide));
                testRectangle = new AlignedRectangle(position, rectangle.Width + 1, rectangle.Height + 1);

            } while (!IsValidLocation(testRectangle));
            AddObject(new Water(this, rectangle.Width, rectangle.Height),
                rectangle.Center);
        }
        private void PlacePlants<T>(double concentration) where T : Obstacle
        {
            double area = Width * Height;
            double nTrees = area * concentration;

            for (int i = 0; i < nTrees; ++i)
            {
                bool isPlaced = false;
                var plant = (T)(typeof(T).GetConstructor(new Type[] { typeof(HungerGamesArena) }).Invoke(new object[] { this }));
                while (!isPlaced)
                {
                    var location = new Point(Random.NextDouble(0, Width), Random.NextDouble(0, Height));
                    if (IsValidLocation(new AlignedRectangle(location, ((AlignedRectangle)(plant.Shape)).Width * 2, ((AlignedRectangle)(plant.Shape)).Height * 2))) // The factor of two keeps the plants far enough apart to avoid impossible locations
                    {
                        // Added to avoid an impossible-to-reach spot that hares could otherwise crouch in
                        if (location.X < 1 || location.Y < 1 || location.X > Width - 1 || location.Y > Height - 1)
                        {
                            continue;
                        }
                        AddObject(plant, location);
                        isPlaced = true;
                    }
                }
            }
        }

        private void PlaceGrass(Point center)
        {
            var plants = GetObjectsOfType<Obstacle>();

            if (Occupied(new AlignedRectangle(center, 1, 1), plants))
                return;

            double width = 1;
            double length = 1;

            bool keepGoing = true;
            while (keepGoing)
            {
                keepGoing = false;
                var rect1 = new AlignedRectangle(center, width + 1, length);
                if (!Occupied(rect1, plants) && TestShape(rect1))
                {
                    width += 1;
                    keepGoing = true;
                }
                var rect2 = new AlignedRectangle(center, width, length + 1);
                if (!Occupied(rect2, plants) && TestShape(rect2))
                {
                    length += 1;
                    keepGoing = true;
                }
            }



            double area = length * width;
            double height = area / areaOfMaxHeight * maxGrassHeight;
            AddObject(new Grass(this, width, length, height), center);
        }

        private bool Occupied(Shape2D rect, IEnumerable<Obstacle> plants)
        {
            foreach (var plant in plants)
            {
                if (plant.Shape.Intersects(rect))
                    return true;
            }
            return false;
        }

        public IEnumerable<string> GetNames<T>() where T : Animal
        {
            HashSet<string> response = [];

            foreach (var ani in GetObjectsOfType<T>())
            {
                response.Add(ani.Name);
            }

            return new List<string>(response);
        }

        private Dictionary<string, int> haresEaten = [];

        public int GetHaresEaten(string name)
        {
            if (haresEaten.TryGetValue(name, out int value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        protected override void UserDefinedEndOfTurn()
        {
            foreach (var lynx in GetObjectsOfType<Lynx>())
            {
                double radius = Math.Max(((AlignedRectangle)(lynx.Shape)).Width, ((AlignedRectangle)(lynx.Shape)).Height);
                foreach (var hare in GetNearbyObjects<Hare>(lynx.Position, radius))
                {
                    if (hare.Overlaps(lynx))
                    {

                        CheckDeath(hare);
                        hare.Dead = true;

                        if (!SameStudentWriter(hare, lynx))
                        {
                            if (!haresEaten.TryGetValue(lynx.Name, out int value))
                            {
                                value = 0;
                                haresEaten.Add(lynx.Name, value);
                            }
                            haresEaten[lynx.Name] = ++value;
                        }
                    }
                }
            }
            foreach (var hare in GetObjectsOfType<Animal>())
            {
                if (hare.Dead)
                {
                    RemoveObjectDelay(hare);
                }
            }
        }

        private void CheckDeath(Hare hare)
        {
            if (GetObjects(hare.Name).Count((x) => !((Animal)x).Dead) <= 1) // Because the last hare hasn't died yet
            {
                if (!TimeToDeath.ContainsKey(hare.Name))
                {
                    TimeToDeath.Add(hare.Name, Time);
                }
            }
        }

        public string GetWinner(string harename1, string harename2)
        {
            var hares1 = CountObjects(harename1);
            var hares2 = CountObjects(harename2);

            if (hares1 == 0 && hares2 == 0)
            {
                if (TimeToDeath[harename1] == TimeToDeath[harename2])
                    return "TIE";
                else
                    return TimeToDeath[harename1] < TimeToDeath[harename2] ? harename2 : harename1;
            }
            else if (hares1 == 0)
            {
                return harename2;
            }
            else if (hares2 == 0)
            {
                return harename1;
            }
            else
            {
                if (hares1 == hares2)
                    return "TIE";
                else
                    return hares1 > hares2 ? harename1 : harename2;
            }
        }

        internal bool SameStudentWriter(Hare hare, Lynx lynx)
        {
            return HareToLynxMapping[hare.SpeciesCode] == lynx.SpeciesCode;
        }

        public Dictionary<int, int> HareToLynxMapping { get; } = [];

        public Dictionary<string, double> TimeToDeath { get; } = [];

        private List<string> hareNames = null;

        public void AddHareNameForEndCondition(string hareName)
        {
            if (hareNames == null)
            {
                hareNames = [];
            }
            hareNames.Add(hareName);
        }

        protected override bool Done()
        {
            if (Time >= maxTime)
            {
                return true;
            }
            if (hareNames == null)
            {
                return !GetObjectsOfType<Hare>().Any();
            }
            else
            {
                foreach (var hare in hareNames)
                {
                    if (CountObjects(hare) <= 0)
                    {
                        return true;
                    }
                }

                // Stop if nothing is moving
                foreach (var animal in GetObjectsOfType<Animal>())
                {
                    if (!animal.Velocity.IsNull)
                    {
                        return false;
                    }
                }
                return true;
            }

        }
    }
}
