using SimulationCore.Arena;
using SimulationCore.Entities;
using SimulationCore.Math;

namespace SimulationCore.AI;

/// <summary>
/// Simple rule-based intelligence for hares
/// </summary>
public class HareIntelligence : IIntelligence
{
    private readonly Random random;

    public HareIntelligence(Random? random = null)
    {
        this.random = random ?? new Random();
    }

    public Decision ChooseAction(Animal animal, ArenaState arena, List<VisibleThing> visibleThings)
    {
        // Find nearest lynx
        var nearestLynx = visibleThings
            .Where(t => t.Type == ThingType.Lynx)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (nearestLynx != null && nearestLynx.Distance < 15f)
        {
            // Run away from lynx
            var escapeDirection = (animal.Position - nearestLynx.Position).Normalized;
            
            // Add some randomness to avoid getting stuck
            var randomOffset = new Vec2(
                (float)(random.NextDouble() * 2 - 1) * 0.3f,
                (float)(random.NextDouble() * 2 - 1) * 0.3f
            );
            
            return Decision.MoveTo((escapeDirection + randomOffset).Normalized);
        }

        // Avoid obstacles
        var nearestObstacle = visibleThings
            .Where(t => t.Type == ThingType.Tree || t.Type == ThingType.Shrub || t.Type == ThingType.Water)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (nearestObstacle != null && nearestObstacle.Distance < 3f)
        {
            // Move away from obstacle
            var avoidDirection = (animal.Position - nearestObstacle.Position).Normalized;
            return Decision.MoveTo(avoidDirection);
        }

        // Random movement when safe
        if (random.NextDouble() < 0.3) // 30% chance to change direction
        {
            var randomDirection = Vec2.FromAngle((float)(random.NextDouble() * 2 * MathF.PI));
            return Decision.MoveTo(randomDirection);
        }

        return Decision.DoNothing;
    }
}

/// <summary>
/// Perceptron-based intelligence for hares
/// </summary>
public class HarePerceptronIntelligence : IIntelligence
{
    private readonly Perceptron perceptron;
    private readonly Random random;

    public HarePerceptronIntelligence(Perceptron perceptron, Random? random = null)
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
        var inputs = new float[10]; // Fixed size input
        
        // Animal's own state
        inputs[0] = animal.Energy / animal.Stats.MaxStamina;
        inputs[1] = animal.Velocity.Magnitude / animal.Stats.MaxSpeed;
        
        // Distance to nearest lynx
        var nearestLynx = visibleThings.Where(t => t.Type == ThingType.Lynx).OrderBy(t => t.Distance).FirstOrDefault();
        inputs[2] = nearestLynx?.Distance / 50f ?? 1f; // Normalized distance
        inputs[3] = nearestLynx?.Direction.X ?? 0f;
        inputs[4] = nearestLynx?.Direction.Y ?? 0f;
        
        // Distance to nearest obstacle
        var nearestObstacle = visibleThings.Where(t => t.Type == ThingType.Tree || t.Type == ThingType.Shrub || t.Type == ThingType.Water).OrderBy(t => t.Distance).FirstOrDefault();
        inputs[5] = nearestObstacle?.Distance / 20f ?? 1f;
        inputs[6] = nearestObstacle?.Direction.X ?? 0f;
        inputs[7] = nearestObstacle?.Direction.Y ?? 0f;
        
        // Arena boundaries
        inputs[8] = MathF.Min(animal.Position.X, arena.Width - animal.Position.X) / (arena.Width / 2);
        inputs[9] = MathF.Min(animal.Position.Y, arena.Height - animal.Position.Y) / (arena.Height / 2);
        
        return inputs;
    }
} 