using Arena;
using DongUtility;
using HungerGames.Animals;
using System;
using System.Drawing;
using System.Linq;

namespace HungerGames.Interface
{
    public class LynxIntelligenceDefault : LynxIntelligence
    {
        public override Color Color { get { return Color.Thistle; } }
        public override string Name { get { return "Default Lynx"; } }
        public override string BitmapFilename { get { return "lynx.jpg"; } }

        public override Turn ChooseAction()
        {
            const double distanceLimit2 = 25;

            var animal = GetClosest(lynx: false);

            if (Geometry.Geometry2D.Point.DistanceSquared(Position, animal.Position) < distanceLimit2)
            {
                Vector2D direction = animal.Position - Position;
                return ChangeVelocity(direction * 5);
            }


            return ChangeVelocity(Vector2D.NullVector());
        }

    }
}
