using Arena;
using DongUtility;
using Geometry.Geometry2D;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using HungerGamesCore.Terrain;
using System;
using System.Collections.Generic;
using static DongUtility.UtilityFunctions;

namespace HungerGames.Animals
{
    public abstract class Animal : IntelligentOrganism
    {
        public Vector2D Velocity { get; set; } = Vector2D.NullVector();

        public AnimalStats Stats { get; }

        internal double Stamina { get; private set; }

        public bool Dead { get; set; } = false;

        public VisibleAnimal VisibleAnimal { get; }

        public Animal(HungerGamesArena arena, Intelligence intel, AnimalStats stats, double width, double height) :
            base(arena, intel, width, height)
        {
            Stats = stats;
            Stamina = Stats.MaxStamina;
            VisibleAnimal = new VisibleAnimal(this);
        }

        protected override void UserDefinedBeginningOfTurn()
        {
            if (Stamina <= 0 && Velocity.Magnitude > Stats.WalkingSpeed)
            {
                Stamina = 0;
                Velocity *= Stats.WalkingSpeed / Velocity.Magnitude;
            }
        }

        private double nextDecisionTime = 0;
        private double lastTime = 0;

        public void AddDecisionTime(double time)
        {
            nextDecisionTime += time;
        }

        protected override Turn UserDefinedChooseAction()
        {
            double deltaT = Arena.Time - lastTime;
            nextDecisionTime -= deltaT;

            if (nextDecisionTime <= 0)
            {
                nextDecisionTime = Stats.StepTime;
                var result = HungerGamesChooseAction();
                return result;
            }
            return null;
        }

        protected override bool DoTurn(Turn turn)
        { 

            double deltaT = Arena.Time - lastTime;
            if (Velocity != Vector2D.NullVector())
            {
                var newPosition = Position + Velocity * deltaT;
                bool result = Arena.MoveObject(this, newPosition);
                if (!result)
                {
                    var newRect = Shape.TranslateToPoint(newPosition);
                    foreach (var obj in Arena.GetNearbyObjects<Water>(Position, .5))
                    {
                        if (newRect.Intersects(obj.Shape))
                        {
                            Dead = true;
                            return false;
                        }
                    }

                    // Adjust edge behavior
                    var newXPosition = Position + new Vector2D(Velocity.X, 0) * deltaT;
                    var newYPosition = Position + new Vector2D(0, Velocity.Y) * deltaT;

                    bool canMoveX = IsPossible(newXPosition);
                    bool canMoveY = IsPossible(newYPosition);

                    if (!canMoveX && !canMoveY)
                    {
                        Velocity = Vector2D.NullVector();
                    }
                    else if (!canMoveX)
                    {
                        Velocity = new Vector2D(0, Velocity.Y);

                    }
                    else if (!canMoveY)
                    {
                        Velocity = new Vector2D(Velocity.X, 0);
                    }

                    newPosition = Position + Velocity * deltaT;
                    Arena.MoveObject(this, newPosition);
                }
                else
                {
                    // Slow down if inside an obstacle
                    var obstacle = GetCurrentObstacle();
                    if (obstacle != null)
                    {
                        var mag = Velocity.Magnitude;
                        if (obstacle.VelocityReduction - mag > -1)
                        {
                            if (mag == 0)
                            {
                                Velocity = new Vector2D(0, 0);
                            }
                            else
                            {
                                // Set minimum speed to 1
                                Velocity /= mag;
                            }
                        }
                        else
                        {
                            Velocity *= (mag - obstacle.VelocityReduction) / mag;
                        }

                    }
                }
            }

            double staminaLost = StaminaRate(Velocity.Magnitude) * deltaT;
            if (staminaLost > 0)
            {
                Stamina -= staminaLost;
                Stamina = Math.Max(0, Stamina);
            }
            else
            {
                Stamina += Stats.StaminaRestoredPerSecond * deltaT;
                Stamina = Math.Min(Stamina, Stats.MaxStamina);
            }

            if (turn == null)
            {
                return false;
            }

            return turn.DoTurn();
        }

        private bool IsPossible(Point point)
        {
            return Arena.TestShape(Shape.TranslateToPoint(point)) && !Arena.IsOccupied(point, this);
        }

        private Obstacle GetCurrentObstacle()
        {
            var potentials = Arena.GetNearbyObjects<Obstacle>(Position, .5);
            foreach (var potential in potentials)
            {
                if (Overlaps(potential))
                {
                    return potential;
                }
            }
            return null;
        }

        private double StaminaRate(double speed)
        {
            double excess = Math.Max(speed - Stats.WalkingSpeed, 0);
            double placement = excess / (Stats.MaxSpeed - Stats.WalkingSpeed);
            return placement * Stats.StaminaPerSecondAtTopSpeed;
        }

        protected override void UserDefinedEndOfTurn()
        {
            lastTime = Arena.Time;
        }

        public IEnumerable<T> GetVisibleObjects<T>() where T : ArenaObject
        {
            foreach (var obj in Arena.GetNearbyObjects<T>(Position, Stats.VisionBase))
            {
                if (obj != this && CanSee(obj, Stats.VisionBase))
                    yield return obj;
            }
        }

        public IEnumerable<T> GetVisibleObjectsSorted<T>() where T : ArenaObject
        {
            var response = new List<T>();
            foreach (var obj in GetVisibleObjects<T>())
            {
                response.Add(obj);
            }

            response.Sort(new AnimalComparer<T>(this));
            return response;
        }

        private class AnimalComparer<T> : IComparer<T> where T : ArenaObject
        {
            private readonly Animal thisAnimal;

            public AnimalComparer(Animal thisAnimal)
            {
                this.thisAnimal = thisAnimal;
            }

            public int Compare(T one, T two)
            {
                double d1 = Distance2(one, thisAnimal);
                double d2 = Distance2(two, thisAnimal);

                return d1.CompareTo(d2);
            }

            private double Distance2(T one, Animal two)
            {
                return Point.DistanceSquared(one.Position, two.Position);
            }
        }

        private bool CanSee(ArenaObject other, double vision)
        {
            double distance2 = Point.DistanceSquared(other.Position, Position);

            return vision * vision > distance2;
        }

        public override bool IsPassable(ArenaObject mover = null)
        {
            return !(mover is Hare && this is Hare);//(mover is Lynx && this is Hare) || (mover is Hare && this is Lynx);
        }

    }
}
