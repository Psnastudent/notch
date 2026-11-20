# Local Storage Design

| Data | Location | Retention | Protection |
|---|---|---|---|
| Settings | `%LOCALAPPDATA%\NotchLite\settings.json` | Until reset or uninstall | Atomic writes; corrupt-file backup |
| Logs | `%LOCALAPPDATA%\NotchLite\Logs` | Bounded in a later logging policy | Local only; no telemetry |
| Clipboard history | Deferred optional module | User-configured | Disabled by default |
| Notification history | Deferred optional module | User-configured | Local only |

Phase 1 has no database. JSON is sufficient for settings; a database is only introduced if future bounded local history requires indexed queries.
