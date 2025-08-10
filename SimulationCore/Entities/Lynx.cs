using SimulationCore.AI;
using SimulationCore.Arena;
using SimulationCore.Math;

namespace SimulationCore.Entities;

/// <summary>
/// Represents a lynx in the simulation - predator animal
/// </summary>
public class Lynx : Animal
{
    public static readonly AnimalStats DefaultStats = new()
    {
        MaxAcceleration = 2.5f,
        MaxSpeed = 10f,
        MaxStamina = 100f,
        StaminaPerSecondAtTopSpeed = 5f,
        StaminaRestoredPerSecond = 1.5f,
        StepTime = 0.3f,
        WalkingSpeed = 0.7f,
        VisionBase = 35f
    };

    public int HaresCaught { get; set; } = 0;

    public Lynx(IIntelligence intelligence, string name = "Lynx") 
        : base(DefaultStats, intelligence)
    {
        Name = name;
    }

    /// <summary>
    /// Check for hare catches and update score
    /// </summary>
    public override void Update(ArenaState arena, float deltaTime)
    {
        base.Update(arena, deltaTime);
        
        if (!IsAlive) return;

        // Check if we caught any hares
        foreach (var hare in arena.Hares)
        {
            if (hare.IsAlive && Vec2.Distance(Position, hare.Position) < 0.3f)
            {
                hare.IsAlive = false;
                HaresCaught++;
            }
        }
    }
} 