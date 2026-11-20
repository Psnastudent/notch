namespace NotchLite.Core.Models;

public sealed record TrackInfo(
    string Title,
    string Artist,
    string Album,
    TimeSpan Duration,
    TimeSpan Position,
    bool IsPlaying,
    string SourceApp = "Spotify",
    string? ArtworkPath = null,
    byte[]? ArtworkBytes = null);

public sealed record SystemHardwareStats(
    int CpuPercent,
    int GpuPercent,
    int RamPercent,
    double RamUsedGb,
    double RamTotalGb,
    long DownloadBytesSec,
    long UploadBytesSec,
    int BatteryPercent,
    bool IsPluggedIn,
    string PowerModeName);

public sealed record FocusTimerModel(
    TimeSpan TimeRemaining,
    TimeSpan TotalTime,
    bool IsRunning,
    string Mode = "25 + 5 min");

public sealed record AudioEndpoint(
    string Id,
    string Name,
    bool IsInput,
    bool IsDefault);

public sealed record ShelfFileItem(
    string FullPath,
    string DisplayName,
    long SizeBytes,
    DateTime AddedAt);

public sealed record NoticeItem(
    string Id,
    string Title,
    string Body,
    string AppName,
    DateTime Timestamp);

public sealed record ClipboardHistoryItem(
    string Id,
    string Text,
    DateTime CopiedAt,
    string FormattedTime);

public enum IslandTab
{
    Music,
    Web,
    Notices,
    Shelf,
    Clipboard,
    Widgets,
    Controls,
    Actions,
    Deck,
    Calls,
    Settings
}
