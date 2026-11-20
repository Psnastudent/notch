using NotchLite.Core.Models;

namespace NotchLite.Core.Interfaces;

public interface ISystemMetricsService
{
    SystemHardwareStats GetCurrentStats();
    event EventHandler<SystemHardwareStats>? StatsUpdated;
    void StartSampling(int intervalMs = 1000);
    void StopSampling();
}

public interface ISystemAudioService
{
    int MasterVolume { get; set; }
    bool IsMuted { get; set; }
    int DisplayBrightness { get; set; }
    bool IsMicMuted { get; set; }
    event EventHandler? AudioSettingsChanged;
    IReadOnlyList<AudioEndpoint> GetOutputDevices();
    IReadOnlyList<AudioEndpoint> GetInputDevices();
    void PlayTestSound(string deviceId);
}

public interface IMediaControllerService
{
    TrackInfo? CurrentTrack { get; }
    event EventHandler<TrackInfo?>? TrackChanged;
    void TogglePlayPause();
    void NextTrack();
    void PreviousTrack();
    void Seek(TimeSpan position);
}

public interface IFocusTimerService
{
    FocusTimerModel CurrentState { get; }
    event EventHandler<FocusTimerModel>? TimerTicked;
    void Start();
    void Pause();
    void Reset();
    void AdjustMinutes(int deltaMinutes);
}

public interface IShelfService
{
    IReadOnlyList<ShelfFileItem> Items { get; }
    event EventHandler? ItemsChanged;
    void AddFiles(IEnumerable<string> paths);
    void RemoveFile(string path);
    void Clear();
}
