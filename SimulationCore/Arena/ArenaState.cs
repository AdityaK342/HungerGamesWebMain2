using SimulationCore.Entities;
using SimulationCore.Math;

namespace SimulationCore.Arena;

/// <summary>
/// Represents the main arena state containing all entities and bounds
/// </summary>
public class ArenaState
{
    public float Width { get; }
    public float Height { get; }
    public float Time { get; set; }

    public List<Hare> Hares { get; } = new();
    public List<Lynx> Lynxes { get; } = new();
    public List<Obstacle> Obstacles { get; } = new();

    public Random Random { get; }

    public ArenaState(float width, float height, int? seed = null)
    {
        Width = width;
        Height = height;
        Random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Check if a position is within arena bounds
    /// </summary>
    public bool IsInBounds(Vec2 position) =>
        position.X >= 0 && position.X <= Width && position.Y >= 0 && position.Y <= Height;

    /// <summary>
    /// Check if a position with radius is fully within bounds
    /// </summary>
    public bool IsInBounds(Vec2 position, float radius) =>
        position.X >= radius && position.X <= Width - radius && 
        position.Y >= radius && position.Y <= Height - radius;

    /// <summary>
    /// Get all animals within a certain distance of a position
    /// </summary>
    public IEnumerable<Animal> GetNearbyAnimals(Vec2 position, float radius)
    {
        var radiusSquared = radius * radius;
        
        foreach (var hare in Hares)
        {
            if (hare.IsAlive && Vec2.DistanceSquared(hare.Position, position) <= radiusSquared)
                yield return hare;
        }
        
        foreach (var lynx in Lynxes)
        {
            if (lynx.IsAlive && Vec2.DistanceSquared(lynx.Position, position) <= radiusSquared)
                yield return lynx;
        }
    }

    /// <summary>
    /// Get obstacles within a certain distance of a position
    /// </summary>
    public IEnumerable<Obstacle> GetNearbyObstacles(Vec2 position, float radius)
    {
        var radiusSquared = radius * radius;
        
        return Obstacles.Where(obs => 
            Vec2.DistanceSquared(obs.Position, position) <= radiusSquared);
    }

    /// <summary>
    /// Check if a position is blocked by obstacles
    /// </summary>
    public bool IsBlocked(Vec2 position, float radius = 0.1f)
    {
        return Obstacles.Any(obs => obs.Intersects(position, radius));
    }

    /// <summary>
    /// Add a hare to the arena
    /// </summary>
    public void AddHare(Hare hare)
    {
        Hares.Add(hare);
    }

    /// <summary>
    /// Add a lynx to the arena
    /// </summary>
    public void AddLynx(Lynx lynx)
    {
        Lynxes.Add(lynx);
    }

    /// <summary>
    /// Add an obstacle to the arena
    /// </summary>
    public void AddObstacle(Obstacle obstacle)
    {
        Obstacles.Add(obstacle);
    }

    /// <summary>
    /// Remove dead animals from the arena
    /// </summary>
    public void RemoveDeadAnimals()
    {
        Hares.RemoveAll(h => !h.IsAlive);
        Lynxes.RemoveAll(l => !l.IsAlive);
    }

    /// <summary>
    /// Get count of alive hares
    /// </summary>
    public int AliveHareCount => Hares.Count(h => h.IsAlive);

    /// <summary>
    /// Get count of alive lynxes
    /// </summary>
    public int AliveLynxCount => Lynxes.Count(l => l.IsAlive);
} 