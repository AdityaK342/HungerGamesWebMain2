namespace SimulationCore.Math;

/// <summary>
/// Simple 2D vector structure for position and velocity calculations
/// </summary>
public readonly struct Vec2
{
    public float X { get; init; }
    public float Y { get; init; }

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vec2 Zero => new(0, 0);

    public float Magnitude => MathF.Sqrt(X * X + Y * Y);
    public float MagnitudeSquared => X * X + Y * Y;

    public Vec2 Normalized
    {
        get
        {
            var mag = Magnitude;
            return mag > 0 ? new Vec2(X / mag, Y / mag) : Zero;
        }
    }

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, float scalar) => new(a.X * scalar, a.Y * scalar);
    public static Vec2 operator *(float scalar, Vec2 a) => new(a.X * scalar, a.Y * scalar);
    public static Vec2 operator /(Vec2 a, float scalar) => new(a.X / scalar, a.Y / scalar);
    public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Y);

    public static bool operator ==(Vec2 a, Vec2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vec2 a, Vec2 b) => !(a == b);

    public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
    public static float Distance(Vec2 a, Vec2 b) => (a - b).Magnitude;
    public static float DistanceSquared(Vec2 a, Vec2 b) => (a - b).MagnitudeSquared;

    public static Vec2 FromAngle(float angle) => new(MathF.Cos(angle), MathF.Sin(angle));
    public float ToAngle() => MathF.Atan2(Y, X);

    public static Vec2 Lerp(Vec2 a, Vec2 b, float t) => a + (b - a) * MathF.Max(0, MathF.Min(1, t));

    public override string ToString() => $"({X:F2}, {Y:F2})";
    
    public override bool Equals(object? obj) => obj is Vec2 other && this == other;
    public override int GetHashCode() => HashCode.Combine(X, Y);
} 