using System.IO;
using NotchLite.Core.Configuration;
using NotchLite.Infrastructure.Storage;
using Xunit;

namespace NotchLite.Application.Tests;

public sealed class JsonSettingsServiceTests
{
    [Fact]
    public async Task LoadAsyncReturnsDefaultsWhenTheSettingsFileDoesNotExist()
    {
        using var directory = new TemporaryDirectory();
        var settings = await new JsonSettingsService(directory.Path).LoadAsync(CancellationToken.None);

        Assert.Equal(AppSettings.CurrentSchemaVersion, settings.SchemaVersion);
        Assert.False(settings.LowResourceMode);
    }

    [Fact]
    public async Task SaveAsyncRoundTripsSettings()
    {
        using var directory = new TemporaryDirectory();
        var service = new JsonSettingsService(directory.Path);
        var settings = new AppSettings { LowResourceMode = true };

        await service.SaveAsync(settings, CancellationToken.None);
        var loaded = await service.LoadAsync(CancellationToken.None);

        Assert.True(loaded.LowResourceMode);
    }

    [Fact]
    public async Task LoadAsyncMovesMalformedJsonAsideAndReturnsDefaults()
    {
        using var directory = new TemporaryDirectory();
        await File.WriteAllTextAsync(Path.Combine(directory.Path, "settings.json"), "{ malformed json");

        var settings = await new JsonSettingsService(directory.Path).LoadAsync(CancellationToken.None);

        Assert.Equal(AppSettings.CurrentSchemaVersion, settings.SchemaVersion);
        Assert.Single(Directory.GetFiles(directory.Path, "*.corrupt.json"));
    }

    [Fact]
    public async Task UpdateAsyncPersistsTransformedSettings()
    {
        using var directory = new TemporaryDirectory();
        var service = new JsonSettingsService(directory.Path);
        var updated = await service.UpdateAsync(
            settings => settings with { LowResourceMode = true },
            CancellationToken.None);

        var loaded = await service.LoadAsync(CancellationToken.None);
        Assert.True(updated.LowResourceMode);
        Assert.True(loaded.LowResourceMode);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"NotchLiteTests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
