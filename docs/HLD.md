# High-Level Design

NotchLite is a modular monolith. WPF windows and view models depend on application services and core interfaces; Windows APIs and storage are isolated in infrastructure projects. The composition root owns dependency injection and never places Windows API calls in controls.

```text
WPF App and View Models
          |
Application services and lifecycle manager
          |
Core models, events, and interfaces
          |
Infrastructure: JSON storage, logging, Win32/DWM/media adapters
          |
Windows 10/11 APIs and local files
```

Phase 1 implements the composition root, settings boundary, lifecycle contracts, and single-instance boundary. UI windows and optional adapters remain out of scope until the foundation passes tests.
