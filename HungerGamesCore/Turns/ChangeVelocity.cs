using Arena;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HungerGames.Animals;
using DongUtility;

namespace HungerGames.Turns
{
    internal class ChangeVelocity : Turn
    {
        private Vector2D deltaV;

        public ChangeVelocity(Animal owner, Vector2D deltaV) :
            base(owner)
        {
            if (deltaV.Magnitude > owner.Stats.MaxAcceleration)
                deltaV *= owner.Stats.MaxAcceleration / deltaV.Magnitude;

            this.deltaV = deltaV;
        }

        public override bool DoTurn()
        {
            var animal = (Animal)Owner;
            animal.Velocity += deltaV;
            if (animal.Velocity.Magnitude > animal.Stats.MaxSpeed)
            {
                animal.Velocity *= animal.Stats.MaxSpeed / animal.Velocity.Magnitude;
            }
            return true;
        }
    }
}
