# Data Flow

```text
User changes a setting
  -> View model (later phase)
  -> ISettingsService.UpdateAsync
  -> AppSettings value
  -> temporary JSON file
  -> atomic replacement of the local settings file

Application starts
  -> ISingleInstanceService.TryAcquireAsync
  -> ISettingsService.LoadAsync
  -> IAppLifecycleService.StartAsync
  -> WPF windows in later phases
```

No application data is sent to a network service.
