# Low-Level Design

## Settings

`ISettingsService` exposes `LoadAsync`, `SaveAsync`, and `UpdateAsync`. `JsonSettingsService` stores an `AppSettings` value in the user local-app-data folder. It applies defaults when no file exists and creates a timestamped backup before replacing malformed JSON.

## Lifecycle

`IAppLifecycleService` exposes asynchronous `StartAsync` and `StopAsync`. Optional modules implement `IManagedModule` with start, stop, suspend, resume, and dispose operations. Phase 1 registers no optional module; the contract is tested independently.

## Single instance

`ISingleInstanceService` uses a user-scoped named mutex. A second launch detects the held mutex, requests the existing process to foreground when that ability exists later, and exits safely in Phase 1.

## Composition

`App.xaml.cs` builds a Microsoft dependency-injection container, configures local rolling logging, resolves `IAppLifecycleService`, and disposes the provider at shutdown.

## Settings schema and recovery

`AppSettings` starts at schema version `1`. Each saved file is serialized to a sibling temporary file, flushed, and atomically replaced. A malformed primary file is renamed with a timestamped `.corrupt.json` suffix before defaults are returned. The service retains one valid prior backup and never writes settings on the UI thread.
