using SimulationCore.AI;
using SimulationCore.Arena;
using SimulationCore.Entities;
using SimulationCore.Math;

namespace SimulationCore.Engine;

/// <summary>
/// Main simulation loop that updates the arena state
/// </summary>
public class SimulationLoop
{
    public ArenaState Arena { get; }
    public bool IsRunning { get; private set; }
    public float DeltaTime { get; set; } = 0.01f; // 10ms timestep by default

    public event Action<FrameData>? FrameUpdated;

    public SimulationLoop(float arenaWidth, float arenaHeight, int? seed = null)
    {
        Arena = new ArenaState(arenaWidth, arenaHeight, seed);
    }

    /// <summary>
    /// Start the simulation
    /// </summary>
    public void Start()
    {
        IsRunning = true;
    }

    /// <summary>
    /// Pause the simulation
    /// </summary>
    public void Pause()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Reset the simulation to initial state
    /// </summary>
    public void Reset()
    {
        Arena.Hares.Clear();
        Arena.Lynxes.Clear();
        Arena.Obstacles.Clear();
        Arena.Time = 0;
        IsRunning = false;
    }

    /// <summary>
    /// Step the simulation forward by one frame
    /// </summary>
    public void Step()
    {
        if (!IsRunning) return;

        Arena.Time += DeltaTime;

        // Update all animals
        foreach (var hare in Arena.Hares.ToList())
        {
            hare.Update(Arena, DeltaTime);
        }

        foreach (var lynx in Arena.Lynxes.ToList())
        {
            lynx.Update(Arena, DeltaTime);
        }

        // Remove dead animals
        Arena.RemoveDeadAnimals();

        // Create frame data for rendering
        var frameData = new FrameData
        {
            Frame = (int)(Arena.Time / DeltaTime),
            Time = Arena.Time,
            ArenaWidth = Arena.Width,
            ArenaHeight = Arena.Height,
            Hares = Arena.Hares.Where(h => h.IsAlive).Select(h => new AnimalData
            {
                X = h.Position.X,
                Y = h.Position.Y,
                VX = h.Velocity.X,
                VY = h.Velocity.Y,
                Energy = h.Energy,
                IsAlive = h.IsAlive,
                Name = h.Name
            }).ToArray(),
            Lynxes = Arena.Lynxes.Where(l => l.IsAlive).Select(l => new AnimalData
            {
                X = l.Position.X,
                Y = l.Position.Y,
                VX = l.Velocity.X,
                VY = l.Velocity.Y,
                Energy = l.Energy,
                IsAlive = l.IsAlive,
                Name = l.Name
            }).ToArray(),
            Obstacles = Arena.Obstacles.Select(o => new ObstacleData
            {
                Type = o.Type.ToString(),
                X = o.Position.X,
                Y = o.Position.Y,
                Width = o.Size.X,
                Height = o.Size.Y
            }).ToArray()
        };

        FrameUpdated?.Invoke(frameData);
    }

    /// <summary>
    /// Add a hare to the simulation
    /// </summary>
    public void AddHare(string name, IIntelligence intelligence, Vec2? position = null)
    {
        var hare = new Hare(intelligence, name);
        
        if (position.HasValue)
        {
            hare.Position = position.Value;
        }
        else
        {
            // Find a random valid position
            hare.Position = FindValidPosition();
        }

        Arena.AddHare(hare);
    }

    /// <summary>
    /// Add a lynx to the simulation
    /// </summary>
    public void AddLynx(string name, IIntelligence intelligence, Vec2? position = null)
    {
        var lynx = new Lynx(intelligence, name);
        
        if (position.HasValue)
        {
            lynx.Position = position.Value;
        }
        else
        {
            // Find a random valid position
            lynx.Position = FindValidPosition();
        }

        Arena.AddLynx(lynx);
    }

    /// <summary>
    /// Add an obstacle to the simulation
    /// </summary>
    public void AddObstacle(ObstacleType type, Vec2 position, Vec2 size)
    {
        var obstacle = new Obstacle(position, size, type);
        Arena.AddObstacle(obstacle);
    }

    /// <summary>
    /// Generate a simple arena with some obstacles
    /// </summary>
    public void GenerateSimpleArena()
    {
        var random = Arena.Random;
        
        // Add some trees
        for (int i = 0; i < 10; i++)
        {
            var position = new Vec2(
                (float)random.NextDouble() * Arena.Width,
                (float)random.NextDouble() * Arena.Height
            );
            AddObstacle(ObstacleType.Tree, position, new Vec2(2f, 2f));
        }

        // Add some shrubs
        for (int i = 0; i < 15; i++)
        {
            var position = new Vec2(
                (float)random.NextDouble() * Arena.Width,
                (float)random.NextDouble() * Arena.Height
            );
            AddObstacle(ObstacleType.Shrub, position, new Vec2(1f, 1f));
        }

        // Add some water
        for (int i = 0; i < 3; i++)
        {
            var position = new Vec2(
                (float)random.NextDouble() * Arena.Width,
                (float)random.NextDouble() * Arena.Height
            );
            AddObstacle(ObstacleType.Water, position, new Vec2(3f, 3f));
        }
    }

    /// <summary>
    /// Find a valid position for spawning an animal
    /// </summary>
    private Vec2 FindValidPosition()
    {
        var random = Arena.Random;
        
        for (int attempts = 0; attempts < 100; attempts++)
        {
            var position = new Vec2(
                (float)random.NextDouble() * Arena.Width,
                (float)random.NextDouble() * Arena.Height
            );

            if (Arena.IsInBounds(position, 0.5f) && !Arena.IsBlocked(position, 0.5f))
            {
                return position;
            }
        }

        // Fallback to center if no valid position found
        return new Vec2(Arena.Width / 2, Arena.Height / 2);
    }

    /// <summary>
    /// Check if simulation should end (all hares dead, etc.)
    /// </summary>
    public bool ShouldEnd()
    {
        return Arena.AliveHareCount == 0 || Arena.AliveLynxCount == 0;
    }
}

/// <summary>
/// Data for a single simulation frame
/// </summary>
public record FrameData
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public float ArenaWidth { get; init; }
    public float ArenaHeight { get; init; }
    public AnimalData[] Hares { get; init; } = Array.Empty<AnimalData>();
    public AnimalData[] Lynxes { get; init; } = Array.Empty<AnimalData>();
    public ObstacleData[] Obstacles { get; init; } = Array.Empty<ObstacleData>();
}

/// <summary>
/// Data for a single animal
/// </summary>
public record AnimalData
{
    public float X { get; init; }
    public float Y { get; init; }
    public float VX { get; init; }
    public float VY { get; init; }
    public float Energy { get; init; }
    public bool IsAlive { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Data for a single obstacle
/// </summary>
public record ObstacleData
{
    public string Type { get; init; } = string.Empty;
    public float X { get; init; }
    public float Y { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
} 