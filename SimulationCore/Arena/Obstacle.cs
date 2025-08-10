using SimulationCore.Math;

namespace SimulationCore.Arena;

/// <summary>
/// Represents a static obstacle in the arena
/// </summary>
public class Obstacle
{
    public Vec2 Position { get; set; }
    public Vec2 Size { get; set; }
    public ObstacleType Type { get; set; }

    public Obstacle(Vec2 position, Vec2 size, ObstacleType type)
    {
        Position = position;
        Size = size;
        Type = type;
    }

    /// <summary>
    /// Check if a position intersects with this obstacle
    /// </summary>
    public bool Intersects(Vec2 position, float radius)
    {
        // Simple AABB collision with circle
        var half = Size / 2;
        var closest = new Vec2(
            MathF.Max(Position.X - half.X, MathF.Min(Position.X + half.X, position.X)),
            MathF.Max(Position.Y - half.Y, MathF.Min(Position.Y + half.Y, position.Y))
        );
        
        return Vec2.DistanceSquared(position, closest) <= radius * radius;
    }

    /// <summary>
    /// Check if this obstacle blocks movement (some obstacles like grass don't block)
    /// </summary>
    public bool BlocksMovement => Type switch
    {
        ObstacleType.Tree => true,
        ObstacleType.Shrub => true,
        ObstacleType.Water => true,
        ObstacleType.Grass => false,
        ObstacleType.Dirt => false,
        _ => false
    };
}

/// <summary>
/// Types of obstacles that can exist in the arena
/// </summary>
public enum ObstacleType
{
    Tree,
    Shrub, 
    Water,
    Grass,
    Dirt
} 