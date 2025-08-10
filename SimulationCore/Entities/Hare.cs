using SimulationCore.AI;
using SimulationCore.Arena;
using SimulationCore.Math;

namespace SimulationCore.Entities;

/// <summary>
/// Represents a hare in the simulation - prey animal
/// </summary>
public class Hare : Animal
{
    public static readonly AnimalStats DefaultStats = new()
    {
        MaxAcceleration = 3f,
        MaxSpeed = 12f,
        MaxStamina = 75f,
        StaminaPerSecondAtTopSpeed = 7f,
        StaminaRestoredPerSecond = 2f,
        StepTime = 0.25f,
        WalkingSpeed = 0.5f,
        VisionBase = 40f
    };

    public Hare(IIntelligence intelligence, string name = "Hare") 
        : base(DefaultStats, intelligence)
    {
        Name = name;
    }

    /// <summary>
    /// Check for lynx collisions - hares die if touched by lynx
    /// </summary>
    protected override void CheckDeath(ArenaState arena)
    {
        base.CheckDeath(arena);
        
        if (!IsAlive) return;

        // Check if caught by lynx
        foreach (var lynx in arena.Lynxes)
        {
            if (lynx.IsAlive && Vec2.Distance(Position, lynx.Position) < 0.3f)
            {
                IsAlive = false;
                return;
            }
        }
    }
} 