using System.Text.Json;
using NotchLite.Core.Configuration;
using NotchLite.Core.Interfaces;

namespace NotchLite.Infrastructure.Storage;

public sealed class JsonSettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.json";
    private const string BackupFileName = "settings.backup.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsDirectory;

    public JsonSettingsService(string settingsDirectory)
    {
        if (string.IsNullOrWhiteSpace(settingsDirectory))
        {
            throw new ArgumentException("A settings directory is required.", nameof(settingsDirectory));
        }

        _settingsDirectory = settingsDirectory;
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_settingsDirectory);

        if (!File.Exists(SettingsFilePath))
        {
            return AppSettings.Default;
        }

        try
        {
            await using var stream = new FileStream(
                SettingsFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(
                stream,
                SerializerOptions,
                cancellationToken);

            if (settings is null || settings.SchemaVersion < 1)
            {
                throw new JsonException("The settings file does not contain a supported schema version.");
            }

            return settings;
        }
        catch (JsonException)
        {
            await MoveCorruptSettingsAsideAsync(cancellationToken);
            return AppSettings.Default;
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.SchemaVersion != AppSettings.CurrentSchemaVersion)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                "Only the current settings schema can be saved.");
        }

        Directory.CreateDirectory(_settingsDirectory);

        var temporaryFilePath = Path.Combine(_settingsDirectory, $"settings.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                             temporaryFilePath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 4096,
                             useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            if (File.Exists(SettingsFilePath))
            {
                File.Replace(temporaryFilePath, SettingsFilePath, BackupFilePath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryFilePath, SettingsFilePath);
            }
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }

    public async Task<AppSettings> UpdateAsync(
        Func<AppSettings, AppSettings> update,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(update);

        var updatedSettings = update(await LoadAsync(cancellationToken));
        ArgumentNullException.ThrowIfNull(updatedSettings);

        await SaveAsync(updatedSettings, cancellationToken);
        return updatedSettings;
    }

    private string SettingsFilePath => Path.Combine(_settingsDirectory, SettingsFileName);

    private string BackupFilePath => Path.Combine(_settingsDirectory, BackupFileName);

    private Task MoveCorruptSettingsAsideAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(SettingsFilePath))
        {
            return Task.CompletedTask;
        }

        var corruptFileName = $"settings.{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.{Guid.NewGuid():N}.corrupt.json";
        File.Move(SettingsFilePath, Path.Combine(_settingsDirectory, corruptFileName));

        return Task.CompletedTask;
    }
}
