namespace NotchLite.Core.Models;

/// <summary>
/// A platform-agnostic rectangle defined by position and size in device-independent pixels.
/// </summary>
public readonly record struct Rect(double X, double Y, double Width, double Height)
{
    /// <summary>Gets a rectangle with all values set to zero.</summary>
    public static Rect Empty => default;

    public double Left => X;
    public double Top => Y;
    public double Right => X + Width;
    public double Bottom => Y + Height;

    /// <summary>Returns <c>true</c> when width and height are both greater than zero.</summary>
    public bool HasArea => Width > 0 && Height > 0;

    /// <summary>Determines whether this rectangle fully contains <paramref name="other"/>.</summary>
    public bool Contains(Rect other) =>
        other.Left >= Left && other.Top >= Top &&
        other.Right <= Right && other.Bottom <= Bottom;

    /// <summary>Determines whether this rectangle overlaps with <paramref name="other"/>.</summary>
    public bool IntersectsWith(Rect other) =>
        Left < other.Right && Right > other.Left &&
        Top < other.Bottom && Bottom > other.Top;
}
