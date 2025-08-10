using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGamesCore.Terrain;
using System;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;

namespace HungerGames
{
    public class HareIntelligenceAditya : HareIntelligence
    {
        public override Color Color { get { return Color.PowderBlue; } }
        public override string Name { get { return "Aditya"; } }
        public override string BitmapFilename { get { return "Kuku_Aditya.png"; } }
        public override Turn ChooseAction()
        {
            const double distanceLimit2 = 25;

            var animal = GetClosest(lynx: true);

            if (Geometry.Geometry2D.Point.DistanceSquared(Position, animal.Position) < distanceLimit2)
            {
                Vector2D direction = animal.Position - Position;
                return ChangeVelocity(-direction * 5);
            }


            return ChangeVelocity(Vector2D.NullVector());
        }
    }
}
