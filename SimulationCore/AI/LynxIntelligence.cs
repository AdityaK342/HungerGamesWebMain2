using SimulationCore.Arena;
using SimulationCore.Entities;
using SimulationCore.Math;

namespace SimulationCore.AI;

/// <summary>
/// Simple rule-based intelligence for lynxes
/// </summary>
public class LynxIntelligence : IIntelligence
{
    private readonly Random random;

    public LynxIntelligence(Random? random = null)
    {
        this.random = random ?? new Random();
    }

    public Decision ChooseAction(Animal animal, ArenaState arena, List<VisibleThing> visibleThings)
    {
        // Find nearest hare
        var nearestHare = visibleThings
            .Where(t => t.Type == ThingType.Hare)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (nearestHare != null)
        {
            // Chase the hare
            var chaseDirection = (nearestHare.Position - animal.Position).Normalized;
            
            // Add some prediction based on distance
            if (nearestHare.Distance > 5f)
            {
                // Add some randomness for more realistic hunting behavior
                var randomOffset = new Vec2(
                    (float)(random.NextDouble() * 2 - 1) * 0.2f,
                    (float)(random.NextDouble() * 2 - 1) * 0.2f
                );
                chaseDirection = (chaseDirection + randomOffset).Normalized;
            }
            
            return Decision.MoveTo(chaseDirection);
        }

        // Avoid obstacles
        var nearestObstacle = visibleThings
            .Where(t => t.Type == ThingType.Tree || t.Type == ThingType.Shrub || t.Type == ThingType.Water)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (nearestObstacle != null && nearestObstacle.Distance < 2f)
        {
            // Move away from obstacle
            var avoidDirection = (animal.Position - nearestObstacle.Position).Normalized;
            return Decision.MoveTo(avoidDirection);
        }

        // Random movement when no hares visible
        if (random.NextDouble() < 0.4) // 40% chance to change direction
        {
            var randomDirection = Vec2.FromAngle((float)(random.NextDouble() * 2 * MathF.PI));
            return Decision.MoveTo(randomDirection);
        }

        return Decision.DoNothing;
    }
}

/// <summary>
/// Perceptron-based intelligence for lynxes
/// </summary>
public class LynxPerceptronIntelligence : IIntelligence
{
    private readonly Perceptron perceptron;
    private readonly Random random;

    public LynxPerceptronIntelligence(Perceptron perceptron, Random? random = null)
    {
        this.perceptron = perceptron;
        this.random = random ?? new Random();
    }

    public Decision ChooseAction(Animal animal, ArenaState arena, List<VisibleThing> visibleThings)
    {
        // Create input vector from visible things
        var inputs = CreateInputVector(animal, arena, visibleThings);
        
        // Process through neural network
        var outputs = perceptron.Process(inputs);
        
        // Convert outputs to movement decision
        if (outputs.Length >= 2)
        {
            var movement = new Vec2(outputs[0], outputs[1]);
            if (movement.Magnitude > 0.1f)
            {
                return Decision.MoveTo(movement.Normalized);
            }
        }

        return Decision.DoNothing;
    }

    private float[] CreateInputVector(Animal animal, ArenaState arena, List<VisibleThing> visibleThings)
    {
        // Create a simple input vector based on nearby objects
        var inputs = new float[12]; // Fixed size input
        
        // Animal's own state
        inputs[0] = animal.Energy / animal.Stats.MaxStamina;
        inputs[1] = animal.Velocity.Magnitude / animal.Stats.MaxSpeed;
        
        // Distance to nearest hare
        var nearestHare = visibleThings.Where(t => t.Type == ThingType.Hare).OrderBy(t => t.Distance).FirstOrDefault();
        inputs[2] = nearestHare?.Distance / 50f ?? 1f; // Normalized distance
        inputs[3] = nearestHare?.Direction.X ?? 0f;
        inputs[4] = nearestHare?.Direction.Y ?? 0f;
        
        // Count of nearby hares
        var nearbyHares = visibleThings.Where(t => t.Type == ThingType.Hare && t.Distance < 20f).Count();
        inputs[5] = MathF.Min(nearbyHares, 10) / 10f; // Normalized count
        
        // Distance to nearest obstacle
        var nearestObstacle = visibleThings.Where(t => t.Type == ThingType.Tree || t.Type == ThingType.Shrub || t.Type == ThingType.Water).OrderBy(t => t.Distance).FirstOrDefault();
        inputs[6] = nearestObstacle?.Distance / 20f ?? 1f;
        inputs[7] = nearestObstacle?.Direction.X ?? 0f;
        inputs[8] = nearestObstacle?.Direction.Y ?? 0f;
        
        // Arena boundaries
        inputs[9] = MathF.Min(animal.Position.X, arena.Width - animal.Position.X) / (arena.Width / 2);
        inputs[10] = MathF.Min(animal.Position.Y, arena.Height - animal.Position.Y) / (arena.Height / 2);
        
        // Hunting success rate (if available)
        inputs[11] = animal is Lynx lynx ? MathF.Min(lynx.HaresCaught, 10) / 10f : 0f;
        
        return inputs;
    }
} 