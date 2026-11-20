namespace NotchLite.Core.Models;

/// <summary>
/// Platform-agnostic information about a display monitor.
/// All coordinate values are in physical (device) pixels.
/// </summary>
public sealed record MonitorInfo
{
    /// <summary>The device name or identifier (e.g. <c>\\.\DISPLAY1</c>).</summary>
    public required string DeviceName { get; init; }

    /// <summary>Full monitor bounds in physical pixels.</summary>
    public required Rect Bounds { get; init; }

    /// <summary>Usable work area (excluding taskbar etc.) in physical pixels.</summary>
    public required Rect WorkArea { get; init; }

    /// <summary>DPI scale factor (1.0 = 96 DPI, 1.5 = 144 DPI, 2.0 = 192 DPI).</summary>
    public double ScaleFactor { get; init; } = 1.0;

    /// <summary>Whether this is the primary monitor.</summary>
    public bool IsPrimary { get; init; }
}
