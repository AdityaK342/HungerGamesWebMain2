using Arena;
using DongUtility;
using HungerGames.Animals;
using HungerGamesCore.Terrain;
using Geometry.Geometry2D;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HungerGames.Interface
{
    public class LynxIntelligenceAditya : LynxIntelligence
    {
        public override System.Drawing.Color Color => System.Drawing.Color.PowderBlue;
        public override string Name => "AdityaLynx";
        public override string BitmapFilename => "lynx_aditya.png";

        private const double MaxAccel = 4.5;
        private const double ObstacleRadius = 6.0;
        private const double WaterRadius = 10.0;
        private const double AvoidWeight = 0.55;
        private const double WaterWeight = 1.4;
        private const double PredictFactor = 0.55;
        private const double MinMoveSqr = 1.0;
        private const int StuckFrames = 12;
        private const double RestThreshold = 12.0;
        private const double ResumeThreshold = 60.0;

        private readonly Dictionary<VisibleAnimal, Vector2D> _lastHarePos = new();
        private Vector2D? _lastLynxPos = null;
        private int _stuckTicks = 0;
        private Vector2D _prevDir = Vector2D.NullVector();

        public override Turn ChooseAction()
        {
            if (Stamina < RestThreshold)
                return ChangeVelocity(Vector2D.NullVector());
            if (Stamina > ResumeThreshold)
                _stuckTicks = 0;

            Vector2D curr = Position.ToVector2D();
            if (_lastLynxPos is Vector2D last)
                _stuckTicks = (curr - last).MagnitudeSquared < MinMoveSqr ? _stuckTicks + 1 : 0;
            _lastLynxPos = curr;

            Vector2D avoid = Vector2D.NullVector();
            foreach (var obs in GetObstaclesSorted())
            {
                double radius = obs is Water ? WaterRadius : ObstacleRadius;
                double dx = curr.X - obs.Position.X;
                double dy = curr.Y - obs.Position.Y;
                double d2 = dx * dx + dy * dy;
                if (d2 == 0 || d2 > radius * radius) continue;

                double weight = obs is Water ? WaterWeight : 1.0;
                avoid += new Vector2D(dx, dy) * (weight / d2);
            }

            VisibleAnimal? target = null;
            double bestScore = double.MinValue;
            var hares = GetOtherAnimals<Hare>().ToList();

            foreach (var hare in hares)
            {
                if (IsMyHare(hare)) continue;

                Vector2D now = hare.Position.ToVector2D();
                _lastHarePos.TryGetValue(hare, out Vector2D lastSeen);

                Vector2D predicted = now + (now - lastSeen) * PredictFactor;
                _lastHarePos[hare] = now;

                double dist = (predicted - curr).Magnitude;
                int cluster = hares.Count(h => !IsMyHare(h) && (h.Position.ToVector2D() - now).MagnitudeSquared <= 25);

                double score = 200 / (1 + dist) + cluster * 8;
                if (score > bestScore)
                {
                    bestScore = score;
                    target = hare;
                }
            }

            Vector2D desired = Vector2D.NullVector();
            if (target != null && _lastHarePos.TryGetValue(target, out Vector2D targetPos))
                desired = (targetPos - curr).UnitVector() * MaxAccel;

            Vector2D steer = desired + avoid * AvoidWeight;

            if (_stuckTicks >= StuckFrames)
            {
                steer = new Vector2D(-_prevDir.Y, _prevDir.X) * MaxAccel;
                _stuckTicks = 0;
            }

            Vector2D blended = (_prevDir * 0.25 + steer.UnitVector() * 0.75).UnitVector();
            _prevDir = blended;

            return ChangeVelocity(blended * MaxAccel);
        }
    }

    internal static class HGExtensions
    {
        public static Vector2D ToVector2D(this Point p) => new Vector2D(p.X, p.Y);
    }
}
