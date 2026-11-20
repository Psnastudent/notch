using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotchLite.Core.Configuration;
using NotchLite.Core.Interfaces;

namespace NotchLite.App.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IslandViewModel _islandViewModel;
    private AppSettings _currentSettings = AppSettings.Default;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "Appearance";

    [ObservableProperty]
    private bool _isPreviewResting = true;

    [ObservableProperty]
    private bool _isPreviewOpen = false;

    // Appearance
    [ObservableProperty]
    private IslandShape _shape = IslandShape.Island;

    [ObservableProperty]
    private int _distanceFromTop = 3;

    [ObservableProperty]
    private RestingContentMode _whenRestingShow = RestingContentMode.NothingJustTheIsland;

    [ObservableProperty]
    private PetType _pet = PetType.None;

    [ObservableProperty]
    private int _sizePercent = 100;

    [ObservableProperty]
    private int _openSizePercent = 100;

    [ObservableProperty]
    private MotionMode _motion = MotionMode.Fluid;

    [ObservableProperty]
    private VisualizerStyle _visualizer = VisualizerStyle.Pixels;

    // General
    [ObservableProperty]
    private string _language = "Same as Windows";

    [ObservableProperty]
    private bool _runAtStartup = false;

    [ObservableProperty]
    private bool _slimStripUnderMaximizedWindows = true;

    [ObservableProperty]
    private OpenTabDefault _whenIslandOpens = OpenTabDefault.LastTab;

    [ObservableProperty]
    private bool _drawWithProcessor = false;

    // Island
    [ObservableProperty]
    private string _monitorTarget = "Main display";

    [ObservableProperty]
    private bool _hiddenUntilHover = false;

    [ObservableProperty]
    private int _openDelayMs = 150;

    [ObservableProperty]
    private bool _reverseScrollingOnIsland = false;

    public IReadOnlyList<string> Categories { get; } = new[]
    {
        "General",
        "Accessibility",
        "Appearance",
        "Island",
        "Taskbar",
        "Panels",
        "Widgets",
        "Wallpaper",
        "Calls",
        "Streaming",
        "Gaming",
        "AI agents",
        "Tools",
        "Keys",
        "About"
    };

    public SettingsViewModel(ISettingsService settingsService, IslandViewModel islandViewModel)
    {
        _settingsService = settingsService;
        _islandViewModel = islandViewModel;
        LoadSettings();
    }

    private async void LoadSettings()
    {
        _currentSettings = await _settingsService.LoadAsync(CancellationToken.None);
        Shape = _currentSettings.Shape;
        DistanceFromTop = _currentSettings.DistanceFromTop;
        WhenRestingShow = _currentSettings.WhenRestingShow;
        Pet = _currentSettings.Pet;
        SizePercent = _currentSettings.SizePercent;
        OpenSizePercent = _currentSettings.OpenSizePercent;
        Motion = _currentSettings.Motion;
        Visualizer = _currentSettings.Visualizer;
        Language = _currentSettings.Language;
        RunAtStartup = _currentSettings.RunAtStartup;
        SlimStripUnderMaximizedWindows = _currentSettings.SlimStripUnderMaximizedWindows;
        WhenIslandOpens = _currentSettings.WhenIslandOpens;
        DrawWithProcessor = _currentSettings.DrawWithProcessor;
        MonitorTarget = _currentSettings.MonitorTarget;
        HiddenUntilHover = _currentSettings.HiddenUntilHover;
        OpenDelayMs = _currentSettings.OpenDelayMs;
        ReverseScrollingOnIsland = _currentSettings.ReverseScrollingOnIsland;
    }

    [RelayCommand]
    public void SetPreviewResting()
    {
        IsPreviewResting = true;
        IsPreviewOpen = false;
    }

    [RelayCommand]
    public void SetPreviewOpen()
    {
        IsPreviewResting = false;
        IsPreviewOpen = true;
    }

    [RelayCommand]
    public void SelectCategory(string category)
    {
        SelectedCategory = category;
    }

    [RelayCommand]
    public void SetShape(string shapeName)
    {
        if (Enum.TryParse<IslandShape>(shapeName, out var s))
        {
            Shape = s;
            _islandViewModel.Shape = s;
            SaveAsync();
        }
    }

    [RelayCommand]
    public void SetPet(string petName)
    {
        if (Enum.TryParse<PetType>(petName, out var p))
        {
            Pet = p;
            _islandViewModel.Pet = p;
            SaveAsync();
        }
    }

    [RelayCommand]
    public void SetMotion(string motionName)
    {
        if (Enum.TryParse<MotionMode>(motionName, out var m))
        {
            Motion = m;
            _islandViewModel.Motion = m;
            SaveAsync();
        }
    }

    [RelayCommand]
    public void SetVisualizer(string visualizerName)
    {
        if (Enum.TryParse<VisualizerStyle>(visualizerName, out var v))
        {
            Visualizer = v;
            _islandViewModel.Visualizer = v;
            SaveAsync();
        }
    }

    [RelayCommand]
    public void SetWhenIslandOpens(string option)
    {
        if (Enum.TryParse<OpenTabDefault>(option, out var o))
        {
            WhenIslandOpens = o;
            SaveAsync();
        }
    }

    public async void SaveAsync()
    {
        _currentSettings = _currentSettings with
        {
            Shape = Shape,
            DistanceFromTop = DistanceFromTop,
            WhenRestingShow = WhenRestingShow,
            Pet = Pet,
            SizePercent = SizePercent,
            OpenSizePercent = OpenSizePercent,
            Motion = Motion,
            Visualizer = Visualizer,
            Language = Language,
            RunAtStartup = RunAtStartup,
            SlimStripUnderMaximizedWindows = SlimStripUnderMaximizedWindows,
            WhenIslandOpens = WhenIslandOpens,
            DrawWithProcessor = DrawWithProcessor,
            MonitorTarget = MonitorTarget,
            HiddenUntilHover = HiddenUntilHover,
            OpenDelayMs = OpenDelayMs,
            ReverseScrollingOnIsland = ReverseScrollingOnIsland
        };

        _islandViewModel.Shape = Shape;
        _islandViewModel.DistanceFromTop = DistanceFromTop;
        _islandViewModel.Pet = Pet;
        _islandViewModel.Motion = Motion;
        _islandViewModel.Visualizer = Visualizer;

        try
        {
            await _settingsService.SaveAsync(_currentSettings, CancellationToken.None);
        }
        catch
        {
            // Ignore failure
        }
    }
}
