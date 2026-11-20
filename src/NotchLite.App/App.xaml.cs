using System.IO;
using System.Security.Principal;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotchLite.App.ViewModels;
using NotchLite.App.Views;
using NotchLite.Application.Services;
using NotchLite.Core.Interfaces;
using NotchLite.Infrastructure.Storage;
using NotchLite.Infrastructure.Windows;
using NotchLite.Platform.Audio;
using NotchLite.Platform.Media;
using NotchLite.Platform.Monitoring;

namespace NotchLite.App;

public partial class App : System.Windows.Application
{
    private readonly object _cleanupLock = new();
    private IAppLifecycleService? _lifecycleService;
    private ILogger<App>? _logger;
    private Task<int>? _cleanupTask;
    private ServiceProvider? _serviceProvider;
    private IslandWindow? _islandWindow;
    private SettingsWindow? _settingsWindow;
    private DockWindow? _dockWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        using var startupCancellation = new CancellationTokenSource();

        try
        {
            _serviceProvider = CreateServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();

            var singleInstanceService = _serviceProvider.GetRequiredService<ISingleInstanceService>();
            if (!await singleInstanceService.TryAcquireAsync(startupCancellation.Token))
            {
                LogMessages.AlreadyRunning(_logger);
                await ShutdownApplicationAsync(0);
                return;
            }

            var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            var settings = await settingsService.LoadAsync(startupCancellation.Token);
            LogMessages.LoadedSettingsSchemaVersion(_logger, settings.SchemaVersion);

            _lifecycleService = _serviceProvider.GetRequiredService<IAppLifecycleService>();
            await _lifecycleService.StartAsync(startupCancellation.Token);
            LogMessages.FoundationStarted(_logger);

            // Instantiate Windows
            _islandWindow = _serviceProvider.GetRequiredService<IslandWindow>();
            _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            _dockWindow = _serviceProvider.GetRequiredService<DockWindow>();

            var islandVm = _serviceProvider.GetRequiredService<IslandViewModel>();
            islandVm.OpenSettingsRequested += (_, _) =>
            {
                _settingsWindow.Show();
                _settingsWindow.Activate();
            };

            var dockVm = _serviceProvider.GetRequiredService<DockViewModel>();
            dockVm.OpenSettingsRequested += (_, _) =>
            {
                _settingsWindow.Show();
                _settingsWindow.Activate();
            };

            dockVm.SelectIslandTabRequested += (_, tab) =>
            {
                islandVm.SelectTab(tab);
                _islandWindow.Show();
                _islandWindow.Activate();
            };

            _islandWindow.Show();
            _dockWindow.Show();

            if (e.Args.Any(argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
            {
                await ShutdownApplicationAsync(0);
            }
        }
        catch (OperationCanceledException) when (startupCancellation.IsCancellationRequested)
        {
            if (_logger is not null)
            {
                LogMessages.StartupCancelled(_logger);
            }

            await ShutdownApplicationAsync(1);
        }
        catch (Exception exception)
        {
            if (_logger is not null)
            {
                LogMessages.StartupFailed(_logger, exception);
            }

            await ShutdownApplicationAsync(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            if (EnsureCleanupAsync().GetAwaiter().GetResult() != 0)
            {
                e.ApplicationExitCode = 1;
            }
        }
        catch (Exception exception)
        {
            if (_logger is not null)
            {
                LogMessages.ShutdownFailed(_logger, exception);
            }
            e.ApplicationExitCode = 1;
        }
        finally
        {
            base.OnExit(e);
        }
    }

    private async Task ShutdownApplicationAsync(int requestedExitCode)
    {
        var cleanupExitCode = await EnsureCleanupAsync();
        Shutdown(cleanupExitCode == 0 ? requestedExitCode : 1);
    }

    private Task<int> EnsureCleanupAsync()
    {
        lock (_cleanupLock)
        {
            return _cleanupTask ??= CleanupAsync();
        }
    }

    private async Task<int> CleanupAsync()
    {
        var cleanupExitCode = 0;

        if (_lifecycleService is not null)
        {
            try
            {
                await _lifecycleService.StopAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                if (_logger is not null)
                {
                    LogMessages.ShutdownFailed(_logger, exception);
                }

                cleanupExitCode = 1;
            }
            finally
            {
                _lifecycleService = null;
            }
        }

        if (_serviceProvider is not null)
        {
            try
            {
                await _serviceProvider.DisposeAsync();
            }
            catch (Exception exception)
            {
                if (_logger is not null)
                {
                    LogMessages.ServiceDisposalFailed(_logger, exception);
                }

                cleanupExitCode = 1;
            }
            finally
            {
                _serviceProvider = null;
            }
        }

        return cleanupExitCode;
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NotchLite");

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Core Infrastructure & Storage
        services.AddSingleton<ISettingsService>(_ => new JsonSettingsService(settingsDirectory));
        services.AddSingleton<ISingleInstanceService>(
            _ => new NamedMutexSingleInstanceService(GetUserScopedMutexName()));
        services.AddSingleton<IAppLifecycleService>(
            serviceProvider => new AppLifecycleService(serviceProvider.GetServices<IManagedModule>()));

        // Platform & System Services
        services.AddSingleton<ISystemMetricsService, SystemMetricsService>();
        services.AddSingleton<ISystemAudioService, SystemAudioService>();
        services.AddSingleton<IMediaControllerService, MediaControllerService>();
        services.AddSingleton<IFocusTimerService, FocusTimerService>();
        services.AddSingleton<IShelfService, ShelfService>();

        // ViewModels
        services.AddSingleton<IslandViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<DockViewModel>();

        // Windows
        services.AddSingleton<IslandWindow>();
        services.AddSingleton<SettingsWindow>();
        services.AddSingleton<DockWindow>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static string GetUserScopedMutexName()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var userScope = identity.User?.Value ?? Environment.UserName;
        return $"Local\\NotchLite.{userScope}";
    }

    private static partial class LogMessages
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "A NotchLite instance is already running.")]
        public static partial void AlreadyRunning(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Loaded settings schema version {SchemaVersion}.")]
        public static partial void LoadedSettingsSchemaVersion(ILogger logger, int schemaVersion);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "NotchLite foundation started.")]
        public static partial void FoundationStarted(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "NotchLite startup was cancelled.")]
        public static partial void StartupCancelled(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "NotchLite could not complete startup.")]
        public static partial void StartupFailed(ILogger logger, Exception exception);

        [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "NotchLite could not complete shutdown cleanly.")]
        public static partial void ShutdownFailed(ILogger logger, Exception exception);

        [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "NotchLite could not dispose application services cleanly.")]
        public static partial void ServiceDisposalFailed(ILogger logger, Exception exception);
    }
}
