# API and Interface Design

## Settings

```csharp
public interface ISettingsService
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
    Task<AppSettings> UpdateAsync(
        Func<AppSettings, AppSettings> update,
        CancellationToken cancellationToken);
}
```

## Lifecycle

```csharp
public interface IManagedModule : IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task SuspendAsync(CancellationToken cancellationToken);
    Task ResumeAsync(CancellationToken cancellationToken);
}

public interface IAppLifecycleService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
```

## Single instance

```csharp
public interface ISingleInstanceService : IAsyncDisposable
{
    ValueTask<bool> TryAcquireAsync(CancellationToken cancellationToken);
}
```

Phase 1 rejects a second launch. Foreground activation through local IPC is deliberately deferred until there is a visible main window to activate.
