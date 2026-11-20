using NotchLite.Application.Services;
using NotchLite.Core.Interfaces;
using Xunit;

namespace NotchLite.Application.Tests;

public sealed class AppLifecycleServiceTests
{
    [Fact]
    public async Task StartAsyncStartsModulesInRegistrationOrder()
    {
        var events = new List<string>();
        var service = new AppLifecycleService(
        [
            new RecordingModule("A", events),
            new RecordingModule("B", events),
            new RecordingModule("C", events)
        ]);

        await service.StartAsync(CancellationToken.None);

        Assert.Equal(["start:A", "start:B", "start:C"], events);
    }

    [Fact]
    public async Task StopAsyncStopsStartedModulesInReverseOrder()
    {
        var events = new List<string>();
        var service = new AppLifecycleService(
        [
            new RecordingModule("A", events),
            new RecordingModule("B", events),
            new RecordingModule("C", events)
        ]);

        await service.StartAsync(CancellationToken.None);
        events.Clear();

        await service.StopAsync(CancellationToken.None);

        Assert.Equal(["stop:C", "stop:B", "stop:A"], events);
    }

    [Fact]
    public async Task StartAsyncStopsPreviouslyStartedModulesWhenALaterModuleFails()
    {
        var events = new List<string>();
        var service = new AppLifecycleService(
        [
            new RecordingModule("A", events),
            new RecordingModule("B", events, startException: new InvalidOperationException("B failed to start.")),
            new RecordingModule("C", events)
        ]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));

        Assert.Equal("B failed to start.", exception.Message);
        Assert.Equal(["start:A", "start:B", "stop:A"], events);
    }

    [Fact]
    public async Task StartAsyncDoesNotStartModulesWhenAlreadyCancelled()
    {
        var events = new List<string>();
        var service = new AppLifecycleService([new RecordingModule("A", events)]);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.StartAsync(cancellationSource.Token));

        Assert.Empty(events);
    }

    [Fact]
    public async Task StartAsyncPreservesTheStartupFailureWhenRollbackAlsoFails()
    {
        var events = new List<string>();
        var service = new AppLifecycleService(
        [
            new RecordingModule("A", events, stopException: new InvalidOperationException("A failed to stop.")),
            new RecordingModule("B", events, startException: new InvalidOperationException("B failed to start."))
        ]);

        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => service.StartAsync(CancellationToken.None));

        Assert.Equal(
            ["B failed to start.", "A failed to stop."],
            exception.InnerExceptions.Select(item => item.Message));
        Assert.Equal(["start:A", "start:B", "stop:A"], events);

        events.Clear();
        await Assert.ThrowsAsync<AggregateException>(() => service.StopAsync(CancellationToken.None));
        Assert.Equal(["stop:A"], events);
    }

    [Fact]
    public async Task StopAsyncAttemptsEveryStartedModuleWhenOneStopFails()
    {
        var events = new List<string>();
        var service = new AppLifecycleService(
        [
            new RecordingModule("A", events),
            new RecordingModule("B", events, stopException: new InvalidOperationException("B failed to stop.")),
            new RecordingModule("C", events)
        ]);

        await service.StartAsync(CancellationToken.None);
        events.Clear();

        var exception = await Assert.ThrowsAsync<AggregateException>(
            () => service.StopAsync(CancellationToken.None));

        Assert.Single(exception.InnerExceptions);
        Assert.Equal("B failed to stop.", exception.InnerExceptions[0].Message);
        Assert.Equal(["stop:C", "stop:B", "stop:A"], events);
    }

    [Fact]
    public async Task StartAsyncIsIdempotentAfterSuccessfulStartup()
    {
        var events = new List<string>();
        var service = new AppLifecycleService([new RecordingModule("A", events)]);

        await service.StartAsync(CancellationToken.None);
        await service.StartAsync(CancellationToken.None);

        Assert.Equal(["start:A"], events);
    }

    [Fact]
    public void DisposeCanBeCalledRepeatedly()
    {
        var service = new AppLifecycleService([]);

        service.Dispose();
        service.Dispose();
    }

    private sealed class RecordingModule : IManagedModule
    {
        private readonly List<string> _events;
        private readonly string _name;
        private readonly Exception? _startException;
        private readonly Exception? _stopException;

        public RecordingModule(
            string name,
            List<string> events,
            Exception? startException = null,
            Exception? stopException = null)
        {
            _name = name;
            _events = events;
            _startException = startException;
            _stopException = stopException;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _events.Add($"start:{_name}");
            return _startException is null
                ? Task.CompletedTask
                : Task.FromException(_startException);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _events.Add($"stop:{_name}");
            return _stopException is null
                ? Task.CompletedTask
                : Task.FromException(_stopException);
        }

        public Task SuspendAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ResumeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
