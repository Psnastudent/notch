using NotchLite.Infrastructure.Windows;
using Xunit;

namespace NotchLite.Application.Tests;

public sealed class NamedMutexSingleInstanceServiceTests
{
    [Fact]
    public async Task TryAcquireAsyncRejectsASecondInstanceForTheSameName()
    {
        var mutexName = CreateMutexName();
        await using var first = new NamedMutexSingleInstanceService(mutexName);
        await using var second = new NamedMutexSingleInstanceService(mutexName);

        var firstAcquired = await first.TryAcquireAsync(CancellationToken.None);
        var secondAcquired = await second.TryAcquireAsync(CancellationToken.None);

        Assert.True(firstAcquired);
        Assert.False(secondAcquired);
    }

    [Fact]
    public async Task DisposeAsyncReleasesTheNameForANewInstance()
    {
        var mutexName = CreateMutexName();
        var first = new NamedMutexSingleInstanceService(mutexName);

        Assert.True(await first.TryAcquireAsync(CancellationToken.None));
        await first.DisposeAsync();

        await using var second = new NamedMutexSingleInstanceService(mutexName);
        Assert.True(await second.TryAcquireAsync(CancellationToken.None));
    }

    [Fact]
    public async Task TryAcquireAsyncHonorsCancellation()
    {
        await using var service = new NamedMutexSingleInstanceService(CreateMutexName());
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.TryAcquireAsync(cancellationSource.Token).AsTask());
    }

    [Fact]
    public async Task DisposeAsyncRequiresTheThreadThatAcquiredTheMutex()
    {
        var service = new NamedMutexSingleInstanceService(CreateMutexName());
        Assert.True(await service.TryAcquireAsync(CancellationToken.None));

        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                service.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (Exception caughtException)
            {
                exception = caughtException;
            }
        });

        thread.Start();
        thread.Join();

        Assert.IsType<InvalidOperationException>(exception);
        await service.DisposeAsync();
    }

    private static string CreateMutexName() => $"Local\\NotchLite.Tests.{Guid.NewGuid():N}";
}
