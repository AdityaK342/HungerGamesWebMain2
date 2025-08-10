using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;
using System;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;

namespace HungerGames
{
    public class LynxPerceptronIntelligenceAditya : LynxPerceptronIntelligence
    {
        public override Color Color { get { return Color.HotPink; } }
        public override string Name { get { return "AdityaLynx"; } }
        public override string BitmapFilename { get { return "lynx_aditya.png"; } }
        protected override string PerceptronFilename => "AdityaLynxPerceptron1.pcp";

        protected override double[] GetInputs()
        {
            var input = new double[8];

            var nearestHare = GetOtherAnimals<Hare>()
                .Where(a => !IsSameSpecies(a))
                .OrderBy(a => (a.Position - Position).MagnitudeSquared)
                .FirstOrDefault();

            bool hareVisible = nearestHare != null;

            Vector2D delta = hareVisible ? (nearestHare.Position - Position) : new Vector2D(0, 0);
            double distance = delta.Magnitude;

            double unitX = distance > 0 ? delta.X / distance : 0;
            double unitY = distance > 0 ? delta.Y / distance : 0;

            // manual dot product: velocity • unit vector to hare
            double alignment = (Velocity.X * unitX) + (Velocity.Y * unitY);

            double staminaRatio = Stamina / 100.0;
            bool staminaLow = Stamina < 30.0;

            bool obstacleNearby = GetObstaclesSorted()
                .Any(o => (o.Position - Position).Magnitude < 3.0);

            input[0] = delta.X;                   // relative X
            input[1] = delta.Y;                   // relative Y
            input[2] = distance;                 // range to hare
            input[3] = staminaRatio;             // normalized stamina
            input[4] = staminaLow ? 1.0 : 0.0;    // stamina low flag
            input[5] = alignment;                // manual dot product
            input[6] = obstacleNearby ? 1.0 : 0.0; // obstacle danger flag
            input[7] = hareVisible ? 1.0 : 0.0;   // visibility flag

            return input;
        }



    }
}
