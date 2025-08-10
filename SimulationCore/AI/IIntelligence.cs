using SimulationCore.Arena;
using SimulationCore.Entities;
using SimulationCore.Math;

namespace SimulationCore.AI;

/// <summary>
/// Interface for animal intelligence implementations
/// </summary>
public interface IIntelligence
{
    Decision ChooseAction(Animal animal, ArenaState arena, List<VisibleThing> visibleThings);
}

/// <summary>
/// Represents a decision made by an animal
/// </summary>
public record Decision
{
    public Vec2 Movement { get; init; } = Vec2.Zero;
    public static Decision DoNothing => new();
    public static Decision MoveTo(Vec2 direction) => new() { Movement = direction };
}

/// <summary>
/// Represents something visible to an animal
/// </summary>
public record VisibleThing
{
    public ThingType Type { get; init; }
    public Vec2 Position { get; init; }
    public Vec2 Direction { get; init; }
    public float Distance { get; init; }
}

/// <summary>
/// Types of things that can be seen
/// </summary>
public enum ThingType
{
    Hare,
    Lynx,
    Tree,
    Shrub,
    Water,
    Grass,
    Dirt
} 