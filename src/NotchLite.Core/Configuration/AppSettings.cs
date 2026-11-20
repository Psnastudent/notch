namespace NotchLite.Core.Configuration;

public enum IslandShape
{
    Island,
    Notch
}

public enum RestingContentMode
{
    NothingJustTheIsland,
    MediaVisualizer,
    Clock,
    Pet
}

public enum PetType
{
    None,
    Cat,
    Dog,
    Bunny
}

public enum MotionMode
{
    Fluid,
    Balanced,
    BatterySaver
}

public enum VisualizerStyle
{
    Pixels,
    Bars
}

public enum OpenTabDefault
{
    LastTab,
    Music
}

public enum PowerMode
{
    Battery,
    Balanced,
    Performance
}

public sealed record WebSiteBookmark(string Title, string Url, string Domain, string ColorHex, string IconKey);

public sealed record AppSettings
{
    public const int CurrentSchemaVersion = 1;

    public static AppSettings Default { get; } = new();

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public bool LowResourceMode { get; init; }

    public int IslandCollapseDelayMilliseconds { get; init; } = 400;

    // Appearance & Layout
    public IslandShape Shape { get; init; } = IslandShape.Island;
    public int DistanceFromTop { get; init; } = 3;
    public RestingContentMode WhenRestingShow { get; init; } = RestingContentMode.NothingJustTheIsland;
    public PetType Pet { get; init; } = PetType.None;
    public int SizePercent { get; init; } = 100;
    public int OpenSizePercent { get; init; } = 100;
    public MotionMode Motion { get; init; } = MotionMode.Fluid;
    public VisualizerStyle Visualizer { get; init; } = VisualizerStyle.Pixels;

    // Island Behavior
    public bool HiddenUntilHover { get; init; }
    public int OpenDelayMs { get; init; } = 150;
    public bool ReverseScrollingOnIsland { get; init; }
    public string MonitorTarget { get; init; } = "Main display";

    // General
    public string Language { get; init; } = "Same as Windows";
    public bool RunAtStartup { get; init; }
    public bool SlimStripUnderMaximizedWindows { get; init; } = true;
    public OpenTabDefault WhenIslandOpens { get; init; } = OpenTabDefault.LastTab;
    public bool DrawWithProcessor { get; init; }

    // Controls state
    public PowerMode SelectedPowerMode { get; init; } = PowerMode.Performance;
    public int BrightnessPercent { get; init; } = 30;
    public int VolumePercent { get; init; } = 100;
    public bool IsWifiEnabled { get; init; } = true;
    public bool IsBluetoothEnabled { get; init; }
    public bool IsNightLightEnabled { get; init; }
    public bool IsFocusEnabled { get; init; }
    public bool IsMuted { get; init; }

    // Bookmarks
    public List<WebSiteBookmark> PinnedSites { get; init; } = new()
    {
        new("YT Music", "https://music.youtube.com", "music.you...", "#CC0000", "Music"),
        new("YouTube", "https://www.youtube.com", "youtube.c...", "#FF0000", "Play"),
        new("Spotify", "https://open.spotify.com", "open.spoti...", "#1DB954", "Spotify")
    };
}
