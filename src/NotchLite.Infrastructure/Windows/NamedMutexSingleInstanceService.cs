using NotchLite.Core.Interfaces;

namespace NotchLite.Infrastructure.Windows;

public sealed class NamedMutexSingleInstanceService : ISingleInstanceService
{
    private readonly string _mutexName;
    private readonly object _sync = new();
    private bool _isDisposed;
    private Mutex? _mutex;
    private int _owningThreadId;
    private bool _ownsMutex;

    public NamedMutexSingleInstanceService(string mutexName)
    {
        if (string.IsNullOrWhiteSpace(mutexName))
        {
            throw new ArgumentException("A named mutex is required.", nameof(mutexName));
        }

        _mutexName = mutexName;
    }

    public ValueTask<bool> TryAcquireAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            if (_mutex is not null)
            {
                return ValueTask.FromResult(_ownsMutex);
            }

            var mutex = new Mutex(initiallyOwned: true, _mutexName, out var createdNew);

            if (!createdNew)
            {
                mutex.Dispose();
                return ValueTask.FromResult(false);
            }

            _mutex = mutex;
            _ownsMutex = true;
            _owningThreadId = Environment.CurrentManagedThreadId;
            return ValueTask.FromResult(true);
        }
    }

    public ValueTask DisposeAsync()
    {
        Mutex? mutex;
        var ownsMutex = false;

        lock (_sync)
        {
            if (_isDisposed)
            {
                return ValueTask.CompletedTask;
            }

            if (_ownsMutex && _owningThreadId != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException(
                    "The named mutex must be disposed from the thread that acquired it.");
            }

            _isDisposed = true;
            mutex = _mutex;
            _mutex = null;
            ownsMutex = _ownsMutex;
            _ownsMutex = false;
            _owningThreadId = 0;
        }

        if (mutex is not null)
        {
            try
            {
                if (ownsMutex)
                {
                    mutex.ReleaseMutex();
                }
            }
            finally
            {
                mutex.Dispose();
            }
        }

        return ValueTask.CompletedTask;
    }
}
