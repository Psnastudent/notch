using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotchLite.Core.Configuration;
using NotchLite.Core.Interfaces;
using NotchLite.Core.Models;
using NotchLite.Platform.Win32;

namespace NotchLite.App.ViewModels;

public sealed partial class IslandViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ISystemMetricsService _metricsService;
    private readonly ISystemAudioService _audioService;
    private readonly IMediaControllerService _mediaService;
    private readonly IFocusTimerService _timerService;
    private readonly IShelfService _shelfService;
    private readonly DispatcherTimer _collapseTimer;
    private readonly DispatcherTimer _visualizerTimer;

    // Appearance & State
    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private IslandTab _currentTab = IslandTab.Music;

    [ObservableProperty]
    private IslandShape _shape = IslandShape.Island;

    [ObservableProperty]
    private int _distanceFromTop = 3;

    [ObservableProperty]
    private RestingContentMode _restingContent = RestingContentMode.NothingJustTheIsland;

    [ObservableProperty]
    private PetType _pet = PetType.None;

    [ObservableProperty]
    private MotionMode _motion = MotionMode.Fluid;

    [ObservableProperty]
    private VisualizerStyle _visualizer = VisualizerStyle.Pixels;

    // Live Pulsing Visualizer Heights
    [ObservableProperty]
    private double _bar1Height = 12;

    [ObservableProperty]
    private double _bar2Height = 18;

    [ObservableProperty]
    private double _bar3Height = 8;

    [ObservableProperty]
    private double _bar4Height = 16;

    [ObservableProperty]
    private double _bar5Height = 10;

    [ObservableProperty]
    private double _bar6Height = 14;

    // Music (Real-time GSMTC media session)
    [ObservableProperty]
    private string _trackTitle = "Nothing playing";

    [ObservableProperty]
    private string _artistName = "No media active on your PC";

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private bool _hasActiveMedia;

    [ObservableProperty]
    private System.Windows.Media.ImageSource? _albumArtImage;

    [ObservableProperty]
    private string _currentTimeString = "0:00";

    [ObservableProperty]
    private string _remainingTimeString = "-0:00";

    [ObservableProperty]
    private double _trackProgressPercent;

    [ObservableProperty]
    private int _volume = 100;

    // Widgets: Calendar, Timer, Weather, System Hardware
    [ObservableProperty]
    private string _calendarDayNumber = "19";

    [ObservableProperty]
    private string _calendarDayName = "THU";

    [ObservableProperty]
    private string _calendarMonthName = "NOV";

    [ObservableProperty]
    private string _calendarWeekString = "Week 47";

    [ObservableProperty]
    private string _focusTimerRemaining = "25:00";

    [ObservableProperty]
    private string _focusTimerMode = "25 + 5 min";

    [ObservableProperty]
    private double _focusTimerPercent = 100.0;

    [ObservableProperty]
    private bool _isFocusTimerRunning;

    [ObservableProperty]
    private string _weatherTemp = "22°";

    [ObservableProperty]
    private string _weatherCondition = "Rain";

    [ObservableProperty]
    private string _weatherHighLow = "H 23° L 21°";

    [ObservableProperty]
    private int _cpuPercent = 15;

    [ObservableProperty]
    private int _gpuPercent = 6;

    [ObservableProperty]
    private int _ramPercent = 90;

    [ObservableProperty]
    private string _networkDownSpeed = "0 B/s";

    [ObservableProperty]
    private string _networkUpSpeed = "0 B/s";

    // Controls
    [ObservableProperty]
    private int _brightnessPercent = 30;

    [ObservableProperty]
    private bool _isWifiOn = true;

    [ObservableProperty]
    private bool _isBluetoothOn;

    [ObservableProperty]
    private bool _isNightLightOn;

    [ObservableProperty]
    private bool _isFocusOn;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private PowerMode _powerMode = PowerMode.Performance;

    // Calls
    [ObservableProperty]
    private bool _isMicMuted;

    [ObservableProperty]
    private string _selectedSpeaker = "Speakers (Realtek(R) Audio)";

    [ObservableProperty]
    private string _selectedMic = "Microphone (Realtek(R) Audio)";

    // Collections
    public ObservableCollection<WebSiteBookmark> Bookmarks { get; } = new();
    public ObservableCollection<ShelfFileItem> ShelfItems { get; } = new();
    public ObservableCollection<ClipboardHistoryItem> ClipboardItems { get; } = new();

    [ObservableProperty]
    private string _newClipboardText = "";

    [ObservableProperty]
    private string _clipboardStatusMessage = "Ready";

    private readonly DispatcherTimer _clipboardTimer;
    private string _lastObservedClipboard = "";

    // Mac-style Caps Lock Indicator
    [ObservableProperty]
    private bool _isCapsLockOn;

    [ObservableProperty]
    private bool _isCapsLockHudVisible;

    [ObservableProperty]
    private string _capsLockText = "CAPS LOCK";

    [ObservableProperty]
    private string _capsLockDotColor = "#32D74B";

    [ObservableProperty]
    private string _capsLockIconColor = "#FFFFFF";

    private readonly DispatcherTimer _capsLockMonitorTimer;
    private DispatcherTimer? _capsLockDismissTimer;
    private bool _lastCapsLockState;

    // Event to request Settings window display
    public event EventHandler? OpenSettingsRequested;

    public IslandViewModel(
        ISettingsService settingsService,
        ISystemMetricsService metricsService,
        ISystemAudioService audioService,
        IMediaControllerService mediaService,
        IFocusTimerService timerService,
        IShelfService shelfService)
    {
        _settingsService = settingsService;
        _metricsService = metricsService;
        _audioService = audioService;
        _mediaService = mediaService;
        _timerService = timerService;
        _shelfService = shelfService;

        _collapseTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };
        _collapseTimer.Tick += (_, _) =>
        {
            _collapseTimer.Stop();
            IsExpanded = false;
        };

        // Live Audio Visualizer Pulse Animation (80ms tick)
        _visualizerTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80)
        };
        _visualizerTimer.Tick += OnVisualizerTick;
        _visualizerTimer.Start();

        // Automatic Clipboard Monitor Timer (500ms poll)
        _clipboardTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _clipboardTimer.Tick += OnClipboardTimerTick;
        _clipboardTimer.Start();

        // Mac-style Caps Lock Indicator Monitor (30ms fast poll)
        _lastCapsLockState = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 1) != 0;
        IsCapsLockOn = _lastCapsLockState;
        _capsLockMonitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        _capsLockMonitorTimer.Tick += OnCapsLockMonitorTick;
        _capsLockMonitorTimer.Start();

        // Initialize Services
        _mediaService.TrackChanged += OnTrackChanged;
        _metricsService.StatsUpdated += OnStatsUpdated;
        _timerService.TimerTicked += OnTimerTicked;
        _shelfService.ItemsChanged += OnShelfItemsChanged;
        _audioService.AudioSettingsChanged += OnAudioSettingsChanged;

        // Sync initial hardware volume & brightness
        Volume = _audioService.MasterVolume;
        IsMuted = _audioService.IsMuted;
        BrightnessPercent = _audioService.DisplayBrightness;
        IsMicMuted = _audioService.IsMicMuted;

        LoadInitialSettings();
    }

    private void OnVisualizerTick(object? sender, EventArgs e)
    {
        if (IsPlaying)
        {
            // Modulate bars dynamically to simulate real audio spectrum
            Bar1Height = Random.Shared.Next(4, 16);
            Bar2Height = Random.Shared.Next(6, 22);
            Bar3Height = Random.Shared.Next(4, 12);
            Bar4Height = Random.Shared.Next(8, 20);
            Bar5Height = Random.Shared.Next(4, 14);
            Bar6Height = Random.Shared.Next(6, 18);
        }
        else
        {
            Bar1Height = 4;
            Bar2Height = 4;
            Bar3Height = 4;
            Bar4Height = 4;
            Bar5Height = 4;
            Bar6Height = 4;
        }
    }

    private async void LoadInitialSettings()
    {
        try
        {
            var s = await _settingsService.LoadAsync(CancellationToken.None);
            Shape = s.Shape;
            DistanceFromTop = s.DistanceFromTop;
            RestingContent = s.WhenRestingShow;
            Pet = s.Pet;
            Motion = s.Motion;
            Visualizer = s.Visualizer;
            BrightnessPercent = s.BrightnessPercent;
            Volume = s.VolumePercent;
            PowerMode = s.SelectedPowerMode;
            IsWifiOn = s.IsWifiEnabled;
            IsBluetoothOn = s.IsBluetoothEnabled;
            IsNightLightOn = s.IsNightLightEnabled;
            IsFocusOn = s.IsFocusEnabled;
            IsMuted = s.IsMuted;

            Bookmarks.Clear();
            foreach (var site in s.PinnedSites)
            {
                Bookmarks.Add(site);
            }

            _collapseTimer.Interval = TimeSpan.FromMilliseconds(s.IslandCollapseDelayMilliseconds);
        }
        catch
        {
            // Fallback safe
        }
    }

    [RelayCommand]
    public void Expand()
    {
        _collapseTimer.Stop();
        IsExpanded = true;
        _metricsService.StartSampling(1000);
    }

    [RelayCommand]
    public void StartCollapseTimer()
    {
        _collapseTimer.Stop();
        _collapseTimer.Start();
        _metricsService.StopSampling();
    }

    [RelayCommand]
    public void CancelCollapseTimer()
    {
        _collapseTimer.Stop();
    }

    [RelayCommand]
    public void SelectTab(IslandTab tab)
    {
        CurrentTab = tab;
        Expand();
    }

    [RelayCommand]
    public void TogglePlayPause()
    {
        _mediaService.TogglePlayPause();
    }

    [RelayCommand]
    public void NextTrack()
    {
        _mediaService.NextTrack();
    }

    [RelayCommand]
    public void PrevTrack()
    {
        _mediaService.PreviousTrack();
    }

    [RelayCommand]
    public void ToggleTimer()
    {
        if (IsFocusTimerRunning)
            _timerService.Pause();
        else
            _timerService.Start();
    }

    [RelayCommand]
    public void AdjustTimer(string delta)
    {
        if (int.TryParse(delta, out var d))
        {
            _timerService.AdjustMinutes(d);
        }
    }

    // Interactive Control Center Commands
    [RelayCommand]
    public void ToggleWifi()
    {
        IsWifiOn = !IsWifiOn;
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:network-wifi") { UseShellExecute = true });
        }
        catch { }
        PersistControlState();
    }

    [RelayCommand]
    public void ToggleBluetooth()
    {
        IsBluetoothOn = !IsBluetoothOn;
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:bluetooth") { UseShellExecute = true });
        }
        catch { }
        PersistControlState();
    }

    [RelayCommand]
    public void ToggleNightLight()
    {
        IsNightLightOn = !IsNightLightOn;
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:nightlight") { UseShellExecute = true });
        }
        catch { }
        PersistControlState();
    }

    [RelayCommand]
    public void ToggleFocus()
    {
        IsFocusOn = !IsFocusOn;
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:quiethours") { UseShellExecute = true });
        }
        catch { }
        PersistControlState();
    }

    [RelayCommand]
    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        _audioService.IsMuted = IsMuted;
        PersistControlState();
    }

    [RelayCommand]
    public void SetPowerMode(string modeName)
    {
        if (Enum.TryParse<PowerMode>(modeName, out var p))
        {
            PowerMode = p;
            PersistControlState();
        }
    }

    [RelayCommand]
    public void SetBrightness(double percent)
    {
        BrightnessPercent = Math.Clamp((int)percent, 0, 100);
        _audioService.DisplayBrightness = BrightnessPercent;
        PersistControlState();
    }

    [RelayCommand]
    public void SetVolume(double percent)
    {
        Volume = Math.Clamp((int)percent, 0, 100);
        _audioService.MasterVolume = Volume;
        PersistControlState();
    }

    // Shelf & File Staging Commands
    public void DropFiles(IEnumerable<string> filePaths)
    {
        _shelfService.AddFiles(filePaths);
    }

    [RelayCommand]
    public static void OpenShelfFile(ShelfFileItem item)
    {
        try
        {
            if (File.Exists(item.FullPath))
            {
                Process.Start(new ProcessStartInfo(item.FullPath) { UseShellExecute = true });
            }
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public void RemoveShelfFile(ShelfFileItem item)
    {
        _shelfService.RemoveFile(item.FullPath);
    }

    [RelayCommand]
    public void ClearShelf()
    {
        _shelfService.Clear();
    }

    [RelayCommand]
    public static void SendToPhone()
    {
        try
        {
            SystemSounds.Asterisk.Play();
        }
        catch
        {
            // Ignore
        }
    }

    // Web & Browser Commands
    [RelayCommand]
    public static void OpenWebSite(WebSiteBookmark site)
    {
        try
        {
            Process.Start(new ProcessStartInfo(site.Url) { UseShellExecute = true });
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public void AddNewWebSite()
    {
        Bookmarks.Add(new WebSiteBookmark("GitHub", "https://github.com", "github.com", "#24292E", "Globe"));
        PersistControlState();
    }

    // Tools & Quick Actions Commands
    [RelayCommand]
    public static void TakeScreenshot()
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "ms-screenclip:") { UseShellExecute = true });
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public static void Snip()
    {
        try
        {
            Process.Start(new ProcessStartInfo("snippingtool.exe") { UseShellExecute = true });
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public static void Record()
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "ms-gamebar:") { UseShellExecute = true });
        }
        catch
        {
            // Ignore
        }
    }

    // Calls Tab Commands
    [RelayCommand]
    public void ToggleMic()
    {
        IsMicMuted = !IsMicMuted;
        try
        {
            SystemSounds.Beep.Play();
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public void TestAudio()
    {
        _audioService.PlayTestSound("spk_1");
    }

    [RelayCommand]
    public static void CheckCamera()
    {
        try
        {
            Process.Start(new ProcessStartInfo("microsoft.windows.camera:") { UseShellExecute = true });
        }
        catch
        {
            // Ignore
        }
    }

    [RelayCommand]
    public void OpenSettings()
    {
        OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PersistControlState()
    {
        Task.Run(async () =>
        {
            try
            {
                await _settingsService.UpdateAsync(s => s with
                {
                    BrightnessPercent = BrightnessPercent,
                    VolumePercent = Volume,
                    IsWifiEnabled = IsWifiOn,
                    IsBluetoothEnabled = IsBluetoothOn,
                    IsNightLightEnabled = IsNightLightOn,
                    IsFocusEnabled = IsFocusOn,
                    IsMuted = IsMuted,
                    SelectedPowerMode = PowerMode,
                    PinnedSites = Bookmarks.ToList()
                }, CancellationToken.None);
            }
            catch
            {
                // Ignore
            }
        });
    }

    private void OnAudioSettingsChanged(object? sender, EventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Volume = _audioService.MasterVolume;
            IsMuted = _audioService.IsMuted;
            IsMicMuted = _audioService.IsMicMuted;
        });
    }

    private void OnTrackChanged(object? sender, TrackInfo? track)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            if (track is not null && !string.IsNullOrWhiteSpace(track.Title))
            {
                TrackTitle = track.Title;
                ArtistName = string.IsNullOrWhiteSpace(track.Artist) ? "Playing" : track.Artist;
                IsPlaying = track.IsPlaying;
                HasActiveMedia = true;

                CurrentTimeString = $"{track.Position.Minutes}:{track.Position.Seconds:D2}";
                var remaining = track.Duration - track.Position;
                RemainingTimeString = remaining > TimeSpan.Zero
                    ? $"-{remaining.Minutes}:{remaining.Seconds:D2}"
                    : "-0:00";

                TrackProgressPercent = track.Duration.TotalSeconds > 0
                    ? Math.Clamp((track.Position.TotalSeconds / track.Duration.TotalSeconds) * 100, 0, 100)
                    : 0;

                if (track.ArtworkBytes is not null && track.ArtworkBytes.Length > 0)
                {
                    try
                    {
                        var bmp = new System.Windows.Media.Imaging.BitmapImage();
                        using var ms = new MemoryStream(track.ArtworkBytes);
                        bmp.BeginInit();
                        bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();
                        bmp.Freeze();
                        AlbumArtImage = bmp;
                    }
                    catch
                    {
                        AlbumArtImage = null;
                    }
                }
                else
                {
                    AlbumArtImage = null;
                }
            }
            else
            {
                TrackTitle = "Nothing playing";
                ArtistName = "No media active on your PC";
                IsPlaying = false;
                HasActiveMedia = false;
                AlbumArtImage = null;
                CurrentTimeString = "0:00";
                RemainingTimeString = "-0:00";
                TrackProgressPercent = 0;
            }
        });
    }

    private void OnStatsUpdated(object? sender, SystemHardwareStats stats)
    {
        CpuPercent = stats.CpuPercent;
        GpuPercent = stats.GpuPercent;
        RamPercent = stats.RamPercent;
        NetworkDownSpeed = FormatSpeed(stats.DownloadBytesSec);
        NetworkUpSpeed = FormatSpeed(stats.UploadBytesSec);
    }

    private void OnTimerTicked(object? sender, FocusTimerModel model)
    {
        FocusTimerRemaining = $"{model.TimeRemaining.Minutes:D2}:{model.TimeRemaining.Seconds:D2}";
        IsFocusTimerRunning = model.IsRunning;
        FocusTimerPercent = model.TotalTime.TotalSeconds > 0
            ? (model.TimeRemaining.TotalSeconds / model.TotalTime.TotalSeconds) * 100.0
            : 0;
    }

    private void OnShelfItemsChanged(object? sender, EventArgs e)
    {
        ShelfItems.Clear();
        foreach (var item in _shelfService.Items)
        {
            ShelfItems.Add(item);
        }
    }

    private void OnClipboardTimerTick(object? sender, EventArgs e)
    {
        try
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                var text = System.Windows.Clipboard.GetText().Trim();
                if (!string.IsNullOrEmpty(text) && text != _lastObservedClipboard)
                {
                    _lastObservedClipboard = text;
                    var now = DateTime.Now;
                    var item = new ClipboardHistoryItem(
                        Guid.NewGuid().ToString("N")[..8],
                        text,
                        now,
                        now.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture));

                    var existing = ClipboardItems.FirstOrDefault(i => i.Text == text);
                    if (existing != null)
                    {
                        ClipboardItems.Remove(existing);
                    }

                    ClipboardItems.Insert(0, item);
                    while (ClipboardItems.Count > 30)
                    {
                        ClipboardItems.RemoveAt(ClipboardItems.Count - 1);
                    }

                    ClipboardStatusMessage = $"Captured {now:h:mm tt}";
                }
            }
        }
        catch
        {
            // Clipboard might be locked by another thread
        }
    }

    private void OnCapsLockMonitorTick(object? sender, EventArgs e)
    {
        try
        {
            var currentState = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 1) != 0;
            if (currentState != _lastCapsLockState)
            {
                _lastCapsLockState = currentState;
                IsCapsLockOn = currentState;
                CapsLockText = currentState ? "CAPS LOCK" : "CAPS OFF";
                CapsLockDotColor = currentState ? "#32D74B" : "#6E717E";
                CapsLockIconColor = currentState ? "#FFFFFF" : "#8E929E";

                // Trigger the HUD popup
                IsCapsLockHudVisible = true;
                _capsLockDismissTimer?.Stop();
                _capsLockDismissTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1300) };
                _capsLockDismissTimer.Tick += (_, _) =>
                {
                    _capsLockDismissTimer.Stop();
                    IsCapsLockHudVisible = false;
                };
                _capsLockDismissTimer.Start();
            }
        }
        catch
        {
            // Fallback safe
        }
    }

    [RelayCommand]
    public void CopyClipboardItem(ClipboardHistoryItem? item)
    {
        if (item == null) return;
        try
        {
            _lastObservedClipboard = item.Text;
            System.Windows.Clipboard.SetText(item.Text);
            ClipboardStatusMessage = "Copied to clipboard!";
        }
        catch
        {
            // Safe fallback
        }
    }

    [RelayCommand]
    public void ClearClipboardHistory()
    {
        ClipboardItems.Clear();
        ClipboardStatusMessage = "History cleared";
    }

    [RelayCommand]
    public void AddCustomClipboardText()
    {
        if (string.IsNullOrWhiteSpace(NewClipboardText)) return;
        try
        {
            var text = NewClipboardText.Trim();
            _lastObservedClipboard = text;
            System.Windows.Clipboard.SetText(text);
            var now = DateTime.Now;
            var item = new ClipboardHistoryItem(
                Guid.NewGuid().ToString("N")[..8],
                text,
                now,
                now.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture));

            var existing = ClipboardItems.FirstOrDefault(i => i.Text == text);
            if (existing != null)
            {
                ClipboardItems.Remove(existing);
            }

            ClipboardItems.Insert(0, item);
            NewClipboardText = "";
            ClipboardStatusMessage = "Copied & added!";
        }
        catch
        {
            // Safe fallback
        }
    }

    private static string FormatSpeed(long bytesPerSec)
    {
        if (bytesPerSec < 1024) return $"{bytesPerSec} B/s";
        if (bytesPerSec < 1024 * 1024) return $"{bytesPerSec / 1024.0:F0} KB/s";
        return $"{bytesPerSec / (1024.0 * 1024.0):F1} MB/s";
    }
}
