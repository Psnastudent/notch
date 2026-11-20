using NotchLite.Core.Interfaces;

namespace NotchLite.Application.Services;

public sealed class AppLifecycleService : IAppLifecycleService, IDisposable
{
    private readonly IManagedModule[] _modules;
    private readonly SemaphoreSlim _operationGate = new(initialCount: 1, maxCount: 1);
    private readonly List<IManagedModule> _startedModules = [];
    private int _isDisposed;
    private bool _isRunning;

    public AppLifecycleService(IEnumerable<IManagedModule> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);
        _modules = modules.ToArray();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_isRunning)
            {
                return;
            }

            if (_startedModules.Count != 0)
            {
                throw new InvalidOperationException(
                    "Cannot start while modules from a previous failed shutdown still require cleanup.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var startedThisAttempt = new List<IManagedModule>(_modules.Length);

            try
            {
                foreach (var module in _modules)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await module.StartAsync(cancellationToken).ConfigureAwait(false);
                    startedThisAttempt.Add(module);
                }

                _startedModules.AddRange(startedThisAttempt);
                _isRunning = true;
            }
            catch (Exception startupException)
            {
                var cleanupResult = await StopModulesAsync(startedThisAttempt).ConfigureAwait(false);
                _startedModules.AddRange(cleanupResult.ModulesStillRunning);

                if (cleanupResult.Exceptions.Count != 0)
                {
                    var exceptions = new List<Exception>(cleanupResult.Exceptions.Count + 1)
                    {
                        startupException
                    };
                    exceptions.AddRange(cleanupResult.Exceptions);

                    throw new AggregateException(
                        "A module failed to start and one or more previously started modules could not be stopped.",
                        exceptions);
                }

                throw;
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Once modules are running, shutdown is deliberately best effort. A cancelled
        // caller must not leave active modules behind, so cleanup runs without its token.
        _ = cancellationToken;
        ThrowIfDisposed();
        await _operationGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_startedModules.Count == 0)
            {
                return;
            }

            var stopResult = await StopModulesAsync(_startedModules).ConfigureAwait(false);
            _startedModules.Clear();
            _startedModules.AddRange(stopResult.ModulesStillRunning);
            _isRunning = false;

            if (stopResult.Exceptions.Count != 0)
            {
                throw new AggregateException(
                    "One or more managed modules could not be stopped.",
                    stopResult.Exceptions);
            }
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        _operationGate.Dispose();
    }

    private static async Task<StopResult> StopModulesAsync(List<IManagedModule> modules)
    {
        var modulesStillRunning = new List<IManagedModule>();
        var exceptions = new List<Exception>();

        for (var index = modules.Count - 1; index >= 0; index--)
        {
            var module = modules[index];

            try
            {
                await module.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                modulesStillRunning.Add(module);
                exceptions.Add(exception);
            }
        }

        modulesStillRunning.Reverse();
        return new StopResult(modulesStillRunning, exceptions);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _isDisposed) != 0, this);
    }

    private sealed record StopResult(
        List<IManagedModule> ModulesStillRunning,
        List<Exception> Exceptions);
}
