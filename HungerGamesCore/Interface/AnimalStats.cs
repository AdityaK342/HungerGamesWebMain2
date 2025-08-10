using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGamesCore.Interface
{
    public class AnimalStats
    {
        public double MaxSpeed { get; internal set; }
        public double MaxAcceleration { get; internal set; }
        public double StepTime { get; internal set; }

        public double MaxStamina { get; internal set; }
        public double WalkingSpeed { get; internal set; }
        public double StaminaPerSecondAtTopSpeed { get; internal set; }
        public double StaminaRestoredPerSecond { get; internal set; }


        public double BaseWalkingVolume { get; internal set; }
        public double MaxWalkingVolume { get; internal set; }
        public double HearingThreshold { get; internal set; }
        public double MaxVocalizationVolume { get; internal set; }

        public double VisionBase { get; internal set; }
    }
}
