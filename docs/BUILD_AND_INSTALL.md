# Build and Install

## Local build prerequisite

The isolated worktree includes a local .NET 10 SDK under `.tooling/dotnet`.

```powershell
& '.\.tooling\dotnet\dotnet.exe' restore .\NotchLite.sln
& '.\.tooling\dotnet\dotnet.exe' build .\NotchLite.sln --configuration Release --no-restore
& '.\.tooling\dotnet\dotnet.exe' test .\NotchLite.sln --configuration Release --no-build
```

## Release installation

Installer packaging is deferred until Phase 10. A release must be self-contained x64 and must not require Visual Studio or a system-wide .NET SDK.
