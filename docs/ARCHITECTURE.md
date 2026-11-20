# Architecture

| Project | Responsibility |
|---|---|
| `NotchLite.App` | WPF entry point, resources, and future windows/view models |
| `NotchLite.Core` | Models, settings contracts, lifecycle contracts, and events |
| `NotchLite.Application` | Lifecycle coordinator and application orchestration |
| `NotchLite.Infrastructure` | JSON settings, logging adapters, and Windows-facing implementations |
| `NotchLite.Platform` | Future Win32, DWM, display, and shell abstractions |

Dependencies flow inward: App -> Application/Infrastructure/Platform -> Core. Core references no other NotchLite project.

## Component interaction

```text
App startup -> Single-instance service -> Settings service -> Lifecycle coordinator
    |                                                       |
    +-> structured logger <---------------------------------+
                                                            |
                                                managed modules (later phases)
```
