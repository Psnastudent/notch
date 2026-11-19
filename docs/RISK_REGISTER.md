# Risk Register

| Risk | Impact | Mitigation |
|---|---|---|
| .NET 10 compatibility is unverified | Windows 10 support may regress | Use `net8.0-windows` until verification is available. |
| No SDK on development host | Build and tests cannot run | Install .NET 8 SDK before implementation verification. |
| DWM effects differ by OS/GPU | Visual degradation | Provide an opaque/translucent fallback. |
| Frequent monitoring drains battery | CPU and battery impact | Start monitoring only for open cards and suspend in Gaming/Low Resource modes. |
| Shell integration can destabilize Explorer | User desktop disruption | Default to a non-replacement floating dock; never kill Explorer. |
| Unsupported system APIs | Feature failure | Isolate adapters and use safe settings-page fallbacks. |
