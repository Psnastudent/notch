namespace NotchLite.Core.Interfaces;

/// <summary>
/// Detects whether a fullscreen application is active on any monitor.
/// </summary>
public interface IFullscreenDetectionService
{
    /// <summary>Gets whether a fullscreen application is currently in the foreground.</summary>
    bool IsFullscreenAppActive { get; }

    /// <summary>Raised when the fullscreen state changes.</summary>
    event EventHandler<bool>? FullscreenStateChanged;
}
