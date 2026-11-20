namespace NotchLite.Core.Interfaces;

public interface IAppLifecycleService
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
