using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGamesCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HungerGames.Interface;
using Geometry.Geometry2D;

namespace HungerGames.Interface
{
    abstract public class LocationChooser
    {
        public Point ChooseLocation(VisibleArena arena, bool hare, int organismNumber)
        {
            try
            {
                Point coord = UserDefinedChooseLocation(arena, hare, organismNumber);
                return Check(arena, coord, hare);
            }
            catch
            {
                return RandomLocation(arena, hare);
            }
        }

        private Point Check(VisibleArena arena, Point coord, bool hare)
        {
            if (CheckPoint(arena, coord, hare))
            {
                return coord;
            }
            else
            {
                return RandomLocation(arena, hare);
            }
        }

        protected Point UserDefinedChooseLocation(VisibleArena arena, bool hare, int organismNumber)
        {
            return RandomLocation(arena, hare);
        }

        private Shape2D GetSize(bool hare)
        {
            var animal = MakeOrganism(null, hare);
            return animal.Shape;
        }

        protected Point RandomLocation(VisibleArena arena, bool hare)
        {
            Point center;
            do
            {
                center = new Point(ArenaEngine.Random.NextDouble(0, arena.Width),
                    ArenaEngine.Random.NextDouble(0, arena.Height));
            } while (!CheckPoint(arena, center, hare));
            return center;
        }

        static protected Random Random => ArenaEngine.Random;

        protected bool CheckPoint(VisibleArena arena, Point location, bool hare)
        {
            var size = GetSize(hare);

            var rect = size.TranslateToPoint(location);

            return arena.TestArea(rect);
        }

        abstract public Animal MakeOrganism(ArenaEngine arena, bool hare);

        public string GetName(bool hare)
        {
            var org = MakeOrganism(null, hare);
            return org.Name;
        }

        protected static Animal MakeOrganism<T>(ArenaEngine arena, bool hare) where T : Intelligence
        {
            if (hare)
                return MakeOrganism(arena, typeof(IntelligentHare<>), typeof(T));
            else
                return MakeOrganism(arena, typeof(IntelligentLynx<>), typeof(T));
        }

        private static Animal MakeOrganism(ArenaEngine arena, Type org, Type intel)
        {
            var constructed = org.MakeGenericType(new Type[] { intel });
            var response = Activator.CreateInstance(constructed, new object[] {arena});
            return (Animal)response;
        }
    }
}
