namespace NotchLite.Core.Models;

/// <summary>A platform-agnostic size defined by width and height in device-independent pixels.</summary>
public readonly record struct Size(double Width, double Height)
{
    /// <summary>Gets a size with all values set to zero.</summary>
    public static Size Empty => default;

    /// <summary>Returns <c>true</c> when width and height are both greater than zero.</summary>
    public bool HasArea => Width > 0 && Height > 0;
}
