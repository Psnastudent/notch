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

- Target `net8.0-windows` until .NET 10 support is verified on the required Windows versions.
- No Electron, Chromium, Python, telemetry, cloud account, or background polling.
- Keep dependencies to CommunityToolkit.Mvvm and Microsoft.Extensions packages.
- Optional modules must not prevent the shell from starting or shutting down.

## Deferred work

Island and dock UI, Windows media, system monitoring, notifications, gaming mode, launcher, clipboard history, installer, and release benchmarking begin in later phases.
