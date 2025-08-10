using SimulationCore.AI;
using SimulationCore.Arena;
using SimulationCore.Math;

namespace SimulationCore.Entities;

/// <summary>
/// Base class for all animals in the simulation
/// </summary>
public abstract class Animal
{
    public Vec2 Position { get; set; }
    public Vec2 Velocity { get; set; }
    public float Energy { get; set; }
    public bool IsAlive { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    
    public AnimalStats Stats { get; }
    public IIntelligence Intelligence { get; }

    private float nextDecisionTime = 0;
    private float lastUpdateTime = 0;

    protected Animal(AnimalStats stats, IIntelligence intelligence)
    {
        Stats = stats;
        Intelligence = intelligence;
        Energy = stats.MaxStamina;
    }

    /// <summary>
    /// Update the animal for this frame
    /// </summary>
    public virtual void Update(ArenaState arena, float deltaTime)
    {
        if (!IsAlive) return;

        lastUpdateTime = arena.Time;
        nextDecisionTime -= deltaTime;

        // Handle energy/stamina
        UpdateEnergy(deltaTime);

        // Make decision if it's time
        if (nextDecisionTime <= 0)
        {
            nextDecisionTime = Stats.StepTime;
            MakeDecision(arena);
        }

        // Apply movement
        ApplyMovement(arena, deltaTime);

        // Check for death conditions
        CheckDeath(arena);
    }

    /// <summary>
    /// Make a decision about what to do next
    /// </summary>
    protected virtual void MakeDecision(ArenaState arena)
    {
        var visibleThings = GetVisibleThings(arena);
        var decision = Intelligence.ChooseAction(this, arena, visibleThings);
        
        // Apply the decision
        if (decision.Movement != Vec2.Zero)
        {
            var desiredVelocity = decision.Movement * Stats.MaxSpeed;
            Velocity = Vec2.Lerp(Velocity, desiredVelocity, Stats.MaxAcceleration * Stats.StepTime);
            
            // Limit velocity by current energy
            if (Energy <= 0 && Velocity.Magnitude > Stats.WalkingSpeed)
            {
                Velocity = Velocity.Normalized * Stats.WalkingSpeed;
            }
        }
    }

    /// <summary>
    /// Get all things this animal can see
    /// </summary>
    protected virtual List<VisibleThing> GetVisibleThings(ArenaState arena)
    {
        var visibleThings = new List<VisibleThing>();
        var visionRange = Stats.VisionBase;

        // Find visible animals
        foreach (var animal in arena.GetNearbyAnimals(Position, visionRange))
        {
            if (animal == this) continue;
            
            var distance = Vec2.Distance(Position, animal.Position);
            var direction = (animal.Position - Position).Normalized;
            
            visibleThings.Add(new VisibleThing
            {
                Type = animal is Hare ? ThingType.Hare : ThingType.Lynx,
                Position = animal.Position,
                Direction = direction,
                Distance = distance
            });
        }

        // Find visible obstacles
        foreach (var obstacle in arena.GetNearbyObstacles(Position, visionRange))
        {
            var distance = Vec2.Distance(Position, obstacle.Position);
            var direction = (obstacle.Position - Position).Normalized;
            
            visibleThings.Add(new VisibleThing
            {
                Type = obstacle.Type switch
                {
                    ObstacleType.Tree => ThingType.Tree,
                    ObstacleType.Shrub => ThingType.Shrub,
                    ObstacleType.Water => ThingType.Water,
                    _ => ThingType.Grass
                },
                Position = obstacle.Position,
                Direction = direction,
                Distance = distance
            });
        }

        return visibleThings;
    }

    /// <summary>
    /// Update energy/stamina based on current activity
    /// </summary>
    protected virtual void UpdateEnergy(float deltaTime)
    {
        var speed = Velocity.Magnitude;
        
        if (speed > Stats.WalkingSpeed)
        {
            // Using stamina when running
            var staminaRate = Stats.StaminaPerSecondAtTopSpeed * (speed / Stats.MaxSpeed);
            Energy -= staminaRate * deltaTime;
        }
        else
        {
            // Restoring stamina when walking or resting
            Energy += Stats.StaminaRestoredPerSecond * deltaTime;
        }

        Energy = MathF.Max(0, MathF.Min(Stats.MaxStamina, Energy));
    }

    /// <summary>
    /// Apply movement and handle collisions
    /// </summary>
    protected virtual void ApplyMovement(ArenaState arena, float deltaTime)
    {
        if (Velocity == Vec2.Zero) return;

        var newPosition = Position + Velocity * deltaTime;
        
        // Check bounds
        if (!arena.IsInBounds(newPosition, 0.1f))
        {
            // Bounce off walls
            if (newPosition.X < 0.1f || newPosition.X > arena.Width - 0.1f)
                Velocity = new Vec2(-Velocity.X, Velocity.Y);
            if (newPosition.Y < 0.1f || newPosition.Y > arena.Height - 0.1f)
                Velocity = new Vec2(Velocity.X, -Velocity.Y);
                
            newPosition = Position + Velocity * deltaTime;
            newPosition = new Vec2(
                MathF.Max(0.1f, MathF.Min(arena.Width - 0.1f, newPosition.X)),
                MathF.Max(0.1f, MathF.Min(arena.Height - 0.1f, newPosition.Y))
            );
        }

        // Check obstacles
        if (!arena.IsBlocked(newPosition, 0.1f))
        {
            Position = newPosition;
        }
        else
        {
            // Try moving in just X or Y direction
            var xPosition = Position + new Vec2(Velocity.X * deltaTime, 0);
            var yPosition = Position + new Vec2(0, Velocity.Y * deltaTime);
            
            if (!arena.IsBlocked(xPosition, 0.1f))
            {
                Position = xPosition;
                Velocity = new Vec2(Velocity.X, 0);
            }
            else if (!arena.IsBlocked(yPosition, 0.1f))
            {
                Position = yPosition;
                Velocity = new Vec2(0, Velocity.Y);
            }
            else
            {
                Velocity = Vec2.Zero;
            }
        }
    }

    /// <summary>
    /// Check if animal should die
    /// </summary>
    protected virtual void CheckDeath(ArenaState arena)
    {
        // Check if in water
        foreach (var obstacle in arena.GetNearbyObstacles(Position, 0.1f))
        {
            if (obstacle.Type == ObstacleType.Water && obstacle.Intersects(Position, 0.1f))
            {
                IsAlive = false;
                return;
            }
        }
    }
}

/// <summary>
/// Statistics for an animal type
/// </summary>
public record AnimalStats
{
    public float MaxAcceleration { get; init; }
    public float MaxSpeed { get; init; }
    public float MaxStamina { get; init; }
    public float StaminaPerSecondAtTopSpeed { get; init; }
    public float StaminaRestoredPerSecond { get; init; }
    public float StepTime { get; init; }
    public float WalkingSpeed { get; init; }
    public float VisionBase { get; init; }
} 