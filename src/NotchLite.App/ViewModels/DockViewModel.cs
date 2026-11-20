using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotchLite.Core.Models;

namespace NotchLite.App.ViewModels;

public sealed record DockItem(
    string Name,
    string IconKey,
    string ExecutableOrUri,
    string BackgroundColorHex,
    bool IsRunning);

public sealed partial class DockViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isAutoHideEnabled;

    public ObservableCollection<DockItem> DockApps { get; } = new();

    public event EventHandler? OpenSettingsRequested;
    public event EventHandler<IslandTab>? SelectIslandTabRequested;

    public DockViewModel()
    {
        // 6 distinct squircle app icons matching image media_1795089545392_bece357d.png exactly:
        // 1. Blue (#0078D4) 4-Square Grid (Apps / Start)
        // 2. Amber (#E8A200) Folder (Explorer / Files)
        // 3. Blue (#1A73E8) Globe (Browser / Web)
        // 4. Green (#1DB954) Music Note (Media / Music)
        // 5. Dark (#18181B) Sliders (Control Center)
        // 6. Slate Gray (#475569) Gear (Settings)
        // All have IsRunning = true so they all show the crisp white dot indicator beneath!
        DockApps.Add(new DockItem("Start", "GeomGrid", "start", "#0078D4", true));
        DockApps.Add(new DockItem("Explorer", "GeomFolder", "explorer.exe", "#E8A200", true));
        DockApps.Add(new DockItem("Browser", "GeomGlobe", "https://google.com", "#1A73E8", true));
        DockApps.Add(new DockItem("Music", "GeomMusic", "island:music", "#1DB954", true));
        DockApps.Add(new DockItem("Controls", "GeomSliders", "island:controls", "#18181B", true));
        DockApps.Add(new DockItem("Settings", "GeomGear", "settings", "#475569", true));
    }

    [RelayCommand]
    public void LaunchApp(DockItem item)
    {
        try
        {
            if (item.ExecutableOrUri == "settings")
            {
                OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (item.ExecutableOrUri == "island:controls")
            {
                SelectIslandTabRequested?.Invoke(this, IslandTab.Controls);
            }
            else if (item.ExecutableOrUri == "island:music")
            {
                SelectIslandTabRequested?.Invoke(this, IslandTab.Music);
            }
            else if (item.ExecutableOrUri == "start")
            {
                Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
            }
            else if (item.ExecutableOrUri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(item.ExecutableOrUri) { UseShellExecute = true });
            }
            else
            {
                Process.Start(new ProcessStartInfo(item.ExecutableOrUri) { UseShellExecute = true });
            }
        }
        catch
        {
            // Ignore failure
        }
    }
}
