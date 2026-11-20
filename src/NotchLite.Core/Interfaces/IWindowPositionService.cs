using NotchLite.Core.Models;

namespace NotchLite.Core.Interfaces;

/// <summary>
/// Positions windows relative to monitor bounds with DPI awareness.
/// All <see cref="nint"/> parameters represent window handles (HWND).
/// </summary>
public interface IWindowPositionService
{
    /// <summary>
    /// Centers the window horizontally at the top edge of the specified monitor.
    /// </summary>
    /// <param name="hwnd">The native window handle.</param>
    /// <param name="monitor">Target monitor information.</param>
    void PositionAtTopCenter(nint hwnd, MonitorInfo monitor);

    /// <summary>
    /// Centers the window horizontally at the bottom edge of the specified monitor.
    /// </summary>
    /// <param name="hwnd">The native window handle.</param>
    /// <param name="monitor">Target monitor information.</param>
    /// <param name="dockSize">Desired size of the dock in device-independent pixels.</param>
    void PositionAtBottomCenter(nint hwnd, MonitorInfo monitor, Size dockSize);

    /// <summary>
    /// Sets or removes the topmost (always-on-top) flag for the specified window.
    /// </summary>
    /// <param name="hwnd">The native window handle.</param>
    /// <param name="topmost">Whether the window should be topmost.</param>
    void SetTopmost(nint hwnd, bool topmost);
}
