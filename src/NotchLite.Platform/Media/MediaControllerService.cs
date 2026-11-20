using Windows.Media.Control;
using Windows.Storage.Streams;
using NotchLite.Core.Interfaces;
using NotchLite.Core.Models;

namespace NotchLite.Platform.Media;

public sealed class MediaControllerService : IMediaControllerService, IDisposable
{
    private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;
    private TrackInfo? _currentTrack;
    private bool _disposed;

    public event EventHandler<TrackInfo?>? TrackChanged;

    public TrackInfo? CurrentTrack => _currentTrack;

    public MediaControllerService()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                _sessionManager.SessionsChanged += OnSessionsChanged;
                await UpdateCurrentSessionAsync();
            }
        }
        catch
        {
            // Windows GSMTC not available
        }
    }

    private async void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        await UpdateCurrentSessionAsync();
    }

    private async void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
    {
        await UpdateCurrentSessionAsync();
    }

    private async Task UpdateCurrentSessionAsync()
    {
        if (_sessionManager == null) return;

        try
        {
            if (_currentSession != null)
            {
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
            }

            _currentSession = _sessionManager.GetCurrentSession();

            if (_currentSession != null)
            {
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;

                await RefreshTrackInfoAsync();
            }
            else
            {
                _currentTrack = null;
                TrackChanged?.Invoke(this, null);
            }
        }
        catch
        {
            _currentTrack = null;
            TrackChanged?.Invoke(this, null);
        }
    }

    private async void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        await RefreshTrackInfoAsync();
    }

    private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
    {
        await RefreshTrackInfoAsync();
    }

    private async void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
    {
        await RefreshTrackInfoAsync();
    }

    private async Task RefreshTrackInfoAsync()
    {
        if (_currentSession == null)
        {
            _currentTrack = null;
            TrackChanged?.Invoke(this, null);
            return;
        }

        try
        {
            var props = await _currentSession.TryGetMediaPropertiesAsync();
            var playback = _currentSession.GetPlaybackInfo();
            var timeline = _currentSession.GetTimelineProperties();

            if (props == null || string.IsNullOrWhiteSpace(props.Title))
            {
                _currentTrack = null;
                TrackChanged?.Invoke(this, null);
                return;
            }

            byte[]? thumbBytes = null;
            if (props.Thumbnail != null)
            {
                try
                {
                    using var stream = await props.Thumbnail.OpenReadAsync();
                    if (stream.Size > 0 && stream.Size < 10_000_000)
                    {
                        thumbBytes = new byte[stream.Size];
                        using var reader = new DataReader(stream);
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(thumbBytes);
                    }
                }
                catch
                {
                    // Thumbnail optional
                }
            }

            var isPlaying = playback?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            var duration = timeline?.EndTime ?? TimeSpan.Zero;
            var position = timeline?.Position ?? TimeSpan.Zero;

            _currentTrack = new TrackInfo(
                Title: props.Title,
                Artist: string.IsNullOrWhiteSpace(props.Artist) ? (props.AlbumArtist ?? "") : props.Artist,
                Album: props.AlbumTitle ?? "",
                Duration: duration,
                Position: position,
                IsPlaying: isPlaying,
                SourceApp: _currentSession.SourceAppUserModelId ?? "Media",
                ArtworkPath: null,
                ArtworkBytes: thumbBytes);

            TrackChanged?.Invoke(this, _currentTrack);
        }
        catch
        {
            _currentTrack = null;
            TrackChanged?.Invoke(this, null);
        }
    }

    public async void TogglePlayPause()
    {
        try
        {
            if (_currentSession != null)
            {
                await _currentSession.TryTogglePlayPauseAsync();
            }
        }
        catch
        {
            // Ignore
        }
    }

    public async void NextTrack()
    {
        try
        {
            if (_currentSession != null)
            {
                await _currentSession.TrySkipNextAsync();
            }
        }
        catch
        {
            // Ignore
        }
    }

    public async void PreviousTrack()
    {
        try
        {
            if (_currentSession != null)
            {
                await _currentSession.TrySkipPreviousAsync();
            }
        }
        catch
        {
            // Ignore
        }
    }

    public async void Seek(TimeSpan position)
    {
        try
        {
            if (_currentSession != null)
            {
                var ticks = (long)position.TotalMilliseconds * 10000;
                await _currentSession.TryChangePlaybackPositionAsync(ticks);
            }
        }
        catch
        {
            // Ignore
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_currentSession != null)
        {
            _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
        }

        if (_sessionManager != null)
        {
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
            _sessionManager.SessionsChanged -= OnSessionsChanged;
        }
    }
}
