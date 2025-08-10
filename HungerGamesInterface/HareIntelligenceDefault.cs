using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGamesCore.Terrain;
using System;
using System.Drawing;
using System.Linq;

namespace HungerGames.Interface
{
    public class HareIntelligenceDefault : HareIntelligence
    {
        public override Color Color => Color.HotPink;
        public override string Name => "Default Hare"; 
        public override string BitmapFilename => "hare.jpg";
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
