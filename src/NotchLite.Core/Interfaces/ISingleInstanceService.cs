namespace NotchLite.Core.Interfaces;

public interface ISingleInstanceService : IAsyncDisposable
{
    ValueTask<bool> TryAcquireAsync(CancellationToken cancellationToken);
}
