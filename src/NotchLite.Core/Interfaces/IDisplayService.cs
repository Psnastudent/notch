using NotchLite.Core.Models;

namespace NotchLite.Core.Interfaces;

/// <summary>
/// Provides information about connected display monitors.
/// </summary>
public interface IDisplayService
{
    /// <summary>Gets information about the primary monitor.</summary>
    Task<MonitorInfo> GetPrimaryMonitorAsync();

    /// <summary>Gets information about all connected monitors.</summary>
    Task<IReadOnlyList<MonitorInfo>> GetAllMonitorsAsync();

    /// <summary>Raised when monitor configuration changes (connect/disconnect, resolution, DPI).</summary>
    event EventHandler? MonitorChanged;
}
