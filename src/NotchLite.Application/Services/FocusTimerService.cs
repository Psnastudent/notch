using System.Timers;
using NotchLite.Core.Interfaces;
using NotchLite.Core.Models;

namespace NotchLite.Application.Services;

public sealed class FocusTimerService : IFocusTimerService, IDisposable
{
    private readonly System.Timers.Timer _timer;
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    private TimeSpan _total = TimeSpan.FromMinutes(25);
    private bool _isRunning;

    public event EventHandler<FocusTimerModel>? TimerTicked;

    public FocusTimerModel CurrentState => new(_remaining, _total, _isRunning, "25 + 5 min");

    public FocusTimerService()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_remaining > TimeSpan.Zero)
        {
            _remaining -= TimeSpan.FromSeconds(1);
        }
        else
        {
            _isRunning = false;
            _timer.Stop();
        }

        TimerTicked?.Invoke(this, CurrentState);
    }

    public void Start()
    {
        if (!_isRunning && _remaining > TimeSpan.Zero)
        {
            _isRunning = true;
            _timer.Start();
            TimerTicked?.Invoke(this, CurrentState);
        }
    }

    public void Pause()
    {
        if (_isRunning)
        {
            _isRunning = false;
            _timer.Stop();
            TimerTicked?.Invoke(this, CurrentState);
        }
    }

    public void Reset()
    {
        _isRunning = false;
        _timer.Stop();
        _remaining = _total;
        TimerTicked?.Invoke(this, CurrentState);
    }

    public void AdjustMinutes(int deltaMinutes)
    {
        var newMinutes = Math.Clamp(_total.TotalMinutes + deltaMinutes, 5, 120);
        _total = TimeSpan.FromMinutes(newMinutes);
        if (!_isRunning)
        {
            _remaining = _total;
        }
        TimerTicked?.Invoke(this, CurrentState);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
