using System.IO;
using NotchLite.Application.Services;
using NotchLite.Platform.Audio;
using NotchLite.Platform.Media;
using NotchLite.Platform.Monitoring;
using Xunit;

namespace NotchLite.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact]
    public void FocusTimerServiceStartsAndPausesCorrectly()
    {
        using var timer = new FocusTimerService();
        Assert.False(timer.CurrentState.IsRunning);
        Assert.Equal(TimeSpan.FromMinutes(25), timer.CurrentState.TimeRemaining);

        timer.Start();
        Assert.True(timer.CurrentState.IsRunning);

        timer.Pause();
        Assert.False(timer.CurrentState.IsRunning);

        timer.AdjustMinutes(5);
        Assert.Equal(TimeSpan.FromMinutes(30), timer.CurrentState.TotalTime);

        timer.Reset();
        Assert.Equal(TimeSpan.FromMinutes(30), timer.CurrentState.TimeRemaining);
    }

    [Fact]
    public void ShelfServiceAddsAndRemovesItems()
    {
        var shelf = new ShelfService();
        Assert.Empty(shelf.Items);

        var tempFile = Path.GetTempFileName();
        try
        {
            shelf.AddFiles([tempFile]);
            Assert.Single(shelf.Items);
            Assert.Equal(tempFile, shelf.Items[0].FullPath);

            shelf.RemoveFile(tempFile);
            Assert.Empty(shelf.Items);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void SystemMetricsServiceSamplesHardwareStatsSafely()
    {
        using var metrics = new SystemMetricsService();
        var stats = metrics.GetCurrentStats();

        Assert.InRange(stats.CpuPercent, 0, 100);
        Assert.InRange(stats.GpuPercent, 0, 100);
        Assert.InRange(stats.RamPercent, 0, 100);
        Assert.InRange(stats.BatteryPercent, 0, 100);
        Assert.True(stats.RamTotalGb > 0);
    }

    [Fact]
    public void MediaControllerServiceTogglesPlaybackAndNavigates()
    {
        using var media = new MediaControllerService();
        // With real Windows GSMTC, CurrentTrack represents real system playback
        // Invoking media control commands should execute cleanly without exceptions
        media.TogglePlayPause();
        media.NextTrack();
        media.PreviousTrack();
        media.Seek(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void SystemAudioServiceProvidesEndpointsAndAdjustsLevels()
    {
        using var audio = new SystemAudioService();
        Assert.NotEmpty(audio.GetOutputDevices());
        Assert.NotEmpty(audio.GetInputDevices());

        audio.MasterVolume = 75;
        Assert.InRange(audio.MasterVolume, 0, 100);
        audio.DisplayBrightness = 50;
        Assert.InRange(audio.DisplayBrightness, 0, 100);
    }
}
