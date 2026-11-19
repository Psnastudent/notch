# NotchLite Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a testable Windows 10/11 WPF foundation with local settings, lifecycle management, and single-instance protection.

**Architecture:** A five-project modular monolith keeps the WPF composition root at the edge, domain contracts in Core, orchestration in Application, and local storage/platform behaviour in Infrastructure and Platform. Settings have no UI dependency and use atomic local writes.

**Tech Stack:** C#, .NET 8 WPF, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection/Logging, System.Text.Json, xUnit.

**Spec:** `docs/REQUIREMENTS.md`, `docs/HLD.md`, `docs/LLD.md`, and `docs/ARCHITECTURE.md`.

## Global Constraints

- Support Windows 10 version 2004+ and Windows 11 x64.
- Target `net8.0-windows`; .NET 10 has not been validated or installed.
- Do not add Electron, Chromium, Python, polling loops, telemetry, or cloud dependencies.
- Keep optional modules lazy and disposable.
- Build and test only after the .NET SDK is installed.

## Review Focus

- Missing or malformed settings must not prevent startup; Task 2 tests both.
- Atomic replacement must leave a valid settings file after a save failure; Task 2 tests the write path.
- A second process must not create duplicate overlay windows; Task 4 tests acquisition failure.
- Startup failure must release owned resources; Task 3 tests stop-after-failure behaviour.
- UI-thread ownership must remain in the WPF project; Task 1 enforces one-way project references.

---

### Task 1: Solution and project boundaries

**Files:**
- Create: `NotchLite.sln`, `src/NotchLite.*/*.csproj`, `src/NotchLite.App/App.xaml`, `src/NotchLite.App/App.xaml.cs`
- Create: `tests/NotchLite.Core.Tests/NotchLite.Core.Tests.csproj`, `tests/NotchLite.Application.Tests/NotchLite.Application.Tests.csproj`

**Interfaces:**
- Produces a buildable `net8.0-windows` WPF app and Core/Application/Infrastructure/Platform class libraries.

- [ ] Write a failing architecture reference test.
- [ ] Run the test to verify the expected projects are absent.
- [ ] Create the solution and project references with Core as the inner dependency.
- [ ] Run `dotnet build NotchLite.sln` and verify it succeeds.

### Task 2: Settings boundary

**Files:**
- Create: `src/NotchLite.Core/Configuration/AppSettings.cs`, `src/NotchLite.Core/Interfaces/ISettingsService.cs`
- Create: `src/NotchLite.Infrastructure/Storage/JsonSettingsService.cs`
- Test: `tests/NotchLite.Core.Tests/AppSettingsTests.cs`, `tests/NotchLite.Application.Tests/JsonSettingsServiceTests.cs`

**Interfaces:**
- Produces `Task<AppSettings> LoadAsync(CancellationToken)`, `Task SaveAsync(AppSettings, CancellationToken)`, and `Task<AppSettings> UpdateAsync(Func<AppSettings, AppSettings>, CancellationToken)`.

- [ ] Write failing tests for defaults, save/load round-trip, and malformed JSON recovery.
- [ ] Run the focused tests and verify failure.
- [ ] Implement immutable settings models and atomic JSON persistence.
- [ ] Run focused tests and verify success.

### Task 3: Lifecycle contracts and coordinator

**Files:**
- Create: `src/NotchLite.Core/Interfaces/IManagedModule.cs`, `src/NotchLite.Core/Interfaces/IAppLifecycleService.cs`
- Create: `src/NotchLite.Application/Services/AppLifecycleService.cs`
- Test: `tests/NotchLite.Application.Tests/AppLifecycleServiceTests.cs`

**Interfaces:**
- Produces `StartAsync(CancellationToken)` and `StopAsync(CancellationToken)` with modules started in order and stopped in reverse order.

- [ ] Write failing lifecycle order, cancellation, and cleanup tests.
- [ ] Run focused tests and verify failure.
- [ ] Implement the coordinator with cancellation and exception-safe cleanup.
- [ ] Run focused tests and verify success.

### Task 4: Single instance and application bootstrap

**Files:**
- Create: `src/NotchLite.Core/Interfaces/ISingleInstanceService.cs`
- Create: `src/NotchLite.Infrastructure/Windows/NamedMutexSingleInstanceService.cs`
- Modify: `src/NotchLite.App/App.xaml.cs`
- Test: `tests/NotchLite.Application.Tests/NamedMutexSingleInstanceServiceTests.cs`

**Interfaces:**
- Produces `ValueTask<bool> TryAcquireAsync(CancellationToken)` and `ValueTask DisposeAsync()`.

- [ ] Write a failing test for rejected second acquisition.
- [ ] Run the focused test and verify failure.
- [ ] Implement the mutex service and WPF DI/logging bootstrap.
- [ ] Run all tests and `dotnet build NotchLite.sln`.

### Task 5: Documentation and status

**Files:**
- Modify: `DEVELOPMENT_STATUS.md`, `README.md`, `docs/BUILD_AND_INSTALL.md`

- [ ] Record the installed SDK version, build output, test totals, and only actual measurements.
- [ ] Mark Phase 1 complete only after a successful build, tests, and clean shutdown smoke test.

## Self-review

Phase 1 requirements are mapped to Tasks 1–5. Visual overlays, monitoring, media, gaming, and release packaging remain deliberately outside this plan and are tracked as later phases. Test coverage addresses persistence, corrupted settings, lifecycle ordering, single-instance rejection, and project dependency boundaries.
