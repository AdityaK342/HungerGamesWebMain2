using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGames.Turns;
using HungerGamesCore.Terrain;
using Geometry.Geometry2D;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace HungerGames
{
    public class HarePerceptronIntelligenceAditya : HarePerceptronIntelligence
    {
        public override Color Color => Color.PowderBlue;
        public override string Name => "Aditya";
        public override string BitmapFilename => "Kuku_Aditya.png";
        protected override string PerceptronFilename => "AdityaHarePerceptronRerun.pcp";

        private Vector2D _prevDir = Vector2D.NullVector();

        protected override double[] GetInputs()
        {
            // Cache lynxes once
            var allLynxes = GetOtherAnimals<Lynx>().ToList();

            // 1. Two closest lynxes
            var lynxList = allLynxes
                .OrderBy(l => (l.Position - Position).MagnitudeSquared)
                .Take(2)
                .ToArray();

            double relX1 = 0, relY1 = 0, invD1 = 0;
            double relX2 = 0, relY2 = 0, invD2 = 0;

            if (lynxList.Length > 0)
            {
                var d = lynxList[0].Position - Position;
                relX1 = Math.Clamp(d.X / 60.0, -1, 1);
                relY1 = Math.Clamp(d.Y / 60.0, -1, 1);
                invD1 = Math.Clamp(100.0 / (d.Magnitude + 1), 0, 1);
            }

            if (lynxList.Length > 1)
            {
                var d = lynxList[1].Position - Position;
                relX2 = Math.Clamp(d.X / 60.0, -1, 1);
                relY2 = Math.Clamp(d.Y / 60.0, -1, 1);
                invD2 = Math.Clamp(100.0 / (d.Magnitude + 1), 0, 1);
            }

            // 2. Threat density
            double threatDensity = allLynxes
                .Count(l => (l.Position - Position).Magnitude < 15) / 5.0;
            threatDensity = Math.Clamp(threatDensity, 0, 1);

            // 3. Obstacle sensing
            Vector2D vel = Velocity == null ? Vector2D.NullVector() : Velocity;
            double waterForward = SignedRadialDistance<Water>(12, vel.UnitVector());
            double waterSide = SignedRadialDistance<Water>(12, new Vector2D(-vel.Y, vel.X).UnitVector());

            // 4. Hare status
            double stamina = Stamina / 100.0;
            double speed = Math.Clamp(vel.Magnitude / 4.0, 0, 1);
            double bias = 1.0;

            // FINAL input vector — must be exactly same shape used during training
            return new[]
            {
        relX1, relY1, invD1,
        relX2, relY2, invD2,
        threatDensity,
        waterForward, waterSide,
        stamina, speed,
        bias
    };
        }


        private double SignedRadialDistance<T>(double radius, Vector2D dir) where T : Obstacle
{
    var hit = GetObstacles<T>()
        .Select(o => (obj: o, v: o.Position - Position))
        .Where(t => (t.v.X * dir.X + t.v.Y * dir.Y) > 0) // only forward
        .OrderBy(t => t.v.MagnitudeSquared)
        .FirstOrDefault();

    if (hit.obj == null)
        return 1.0;

    double d = hit.v.Magnitude;
    return Math.Clamp((d - radius) / radius, -1, 1);
}

    }
}