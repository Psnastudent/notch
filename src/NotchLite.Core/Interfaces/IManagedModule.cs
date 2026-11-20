namespace NotchLite.Core.Interfaces;

public interface IManagedModule : IAsyncDisposable
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    Task SuspendAsync(CancellationToken cancellationToken);

    Task ResumeAsync(CancellationToken cancellationToken);
}
