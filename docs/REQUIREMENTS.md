# NotchLite Requirements

## Purpose

NotchLite is a local-first, lightweight Windows desktop utility with a top-centre Dynamic Island and bottom-centre glass dock. It targets Windows 10 version 2004+ and Windows 11 on x64 integrated-graphics PCs.

## Phase 1 scope

- WPF solution with MVVM, dependency injection, structured logging, and nullable reference types.
- Local JSON settings with defaults, atomic persistence, and recovery from unreadable files.
- Application lifecycle with orderly startup and shutdown.
- A single-instance guard.
- Unit tests for settings and lifecycle behaviour.

## Constraints

- Target `net10.0-windows`; .NET 10 is the supported LTS line. Official support is limited to supported Windows 11 releases and specified Windows 10 LTSC/Enterprise releases, so Windows 10 consumer editions require explicit compatibility testing and must not be represented as Microsoft-supported by default.
- No Electron, Chromium, Python, telemetry, cloud account, or background polling.
- Keep dependencies to CommunityToolkit.Mvvm and Microsoft.Extensions packages.
- Optional modules must not prevent the shell from starting or shutting down.

## Deferred work

Island and dock UI, Windows media, system monitoring, notifications, gaming mode, launcher, clipboard history, installer, and release benchmarking begin in later phases.
