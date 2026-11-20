using System.Management;
using System.Media;
using NAudio.CoreAudioApi;
using NotchLite.Core.Interfaces;
using NotchLite.Core.Models;

namespace NotchLite.Platform.Audio;

public sealed class SystemAudioService : ISystemAudioService, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator;
    private MMDevice? _playbackDevice;
    private MMDevice? _captureDevice;
    private int _displayBrightness = 50;
    private bool _disposed;

    public event EventHandler? AudioSettingsChanged;

    public SystemAudioService()
    {
        _enumerator = new MMDeviceEnumerator();
        InitDevices();
        ReadInitialBrightness();
    }

    private void InitDevices()
    {
        try
        {
            _playbackDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (_playbackDevice?.AudioEndpointVolume != null)
            {
                _playbackDevice.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotificationReceived;
            }
        }
        catch
        {
            _playbackDevice = null;
        }

        try
        {
            _captureDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        }
        catch
        {
            _captureDevice = null;
        }
    }

    private void OnVolumeNotificationReceived(AudioVolumeNotificationData data)
    {
        AudioSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int MasterVolume
    {
        get
        {
            try
            {
                if (_playbackDevice?.AudioEndpointVolume != null)
                {
                    return (int)Math.Round(_playbackDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
            }
            catch
            {
                // Fallback safe
            }
            return 100;
        }
        set
        {
            try
            {
                if (_playbackDevice?.AudioEndpointVolume != null)
                {
                    var clamped = (float)Math.Clamp(value / 100.0, 0.0, 1.0);
                    _playbackDevice.AudioEndpointVolume.MasterVolumeLevelScalar = clamped;
                    if (clamped > 0 && _playbackDevice.AudioEndpointVolume.Mute)
                    {
                        _playbackDevice.AudioEndpointVolume.Mute = false;
                    }
                }
            }
            catch
            {
                // Fallback safe
            }
        }
    }

    public bool IsMuted
    {
        get
        {
            try
            {
                return _playbackDevice?.AudioEndpointVolume?.Mute ?? false;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            try
            {
                if (_playbackDevice?.AudioEndpointVolume != null)
                {
                    _playbackDevice.AudioEndpointVolume.Mute = value;
                }
            }
            catch
            {
                // Fallback safe
            }
        }
    }

    public bool IsMicMuted
    {
        get
        {
            try
            {
                return _captureDevice?.AudioEndpointVolume?.Mute ?? false;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            try
            {
                if (_captureDevice?.AudioEndpointVolume != null)
                {
                    _captureDevice.AudioEndpointVolume.Mute = value;
                }
            }
            catch
            {
                // Fallback safe
            }
        }
    }

    public int DisplayBrightness
    {
        get => _displayBrightness;
        set
        {
            _displayBrightness = Math.Clamp(value, 0, 100);
            var target = _displayBrightness;
            _ = Task.Run(() => SetHardwareBrightness(target));
        }
    }

    private void ReadInitialBrightness()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
            foreach (ManagementObject obj in searcher.Get())
            {
                _displayBrightness = Convert.ToInt32(obj["CurrentBrightness"], System.Globalization.CultureInfo.InvariantCulture);
                break;
            }
        }
        catch
        {
            _displayBrightness = 50;
        }
    }

    private static void SetHardwareBrightness(int targetBrightness)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorBrightnessMethods");
            foreach (ManagementObject obj in searcher.Get())
            {
                obj.InvokeMethod("WmiSetBrightness", new object[] { 1u, (byte)targetBrightness });
                break;
            }
        }
        catch
        {
            // Ignore if display does not support WMI brightness
        }
    }

    public IReadOnlyList<AudioEndpoint> GetOutputDevices()
    {
        var list = new List<AudioEndpoint>();
        try
        {
            var defaultId = _playbackDevice?.ID;
            foreach (var dev in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                list.Add(new AudioEndpoint(dev.ID, dev.FriendlyName, IsInput: false, IsDefault: dev.ID == defaultId));
            }
        }
        catch
        {
            // Fallback
        }

        if (list.Count == 0)
        {
            list.Add(new AudioEndpoint("def_out", "Speakers (Realtek(R) Audio)", IsInput: false, IsDefault: true));
        }
        return list;
    }

    public IReadOnlyList<AudioEndpoint> GetInputDevices()
    {
        var list = new List<AudioEndpoint>();
        try
        {
            var defaultId = _captureDevice?.ID;
            foreach (var dev in _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                list.Add(new AudioEndpoint(dev.ID, dev.FriendlyName, IsInput: true, IsDefault: dev.ID == defaultId));
            }
        }
        catch
        {
            // Fallback
        }

        if (list.Count == 0)
        {
            list.Add(new AudioEndpoint("def_in", "Microphone (Realtek(R) Audio)", IsInput: true, IsDefault: true));
        }
        return list;
    }

    public void PlayTestSound(string deviceId)
    {
        try
        {
            SystemSounds.Asterisk.Play();
        }
        catch
        {
            // Ignore failure
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_playbackDevice?.AudioEndpointVolume != null)
        {
            _playbackDevice.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotificationReceived;
        }
        _playbackDevice?.Dispose();
        _captureDevice?.Dispose();
        _enumerator.Dispose();
    }
}
