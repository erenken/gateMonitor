---
applyTo: "dotnet/**"
---

# .NET Architecture — GateMonitor

## Solution Layout

```
dotnet/
├── GateMonitor.AppHost/        Aspire orchestrator — dev entry point
├── GateMonitor.Blazor/         Blazor WASM SPA — Components/, wwwroot/
├── myNOC.Remootio/             .NET library → NuGet package
└── tests/
    ├── myNOC.Tests.Remootio/   MSTest + NSubstitute — library unit tests
    └── GateMonitor.Tests.Blazor/ MSTest + bUnit + NSubstitute — component tests
```

## Architecture Boundaries

- **`myNOC.Remootio`** owns ALL Remootio protocol logic: WebSocket lifecycle, AES-CBC/HMAC-SHA256 crypto, authentication flow, gate state mapping.
  - Public surface: `IRemootioService`, `RemootioService`, `GateState`, `RemootioDeviceConfig`
  - Internal: `RemootioDevice`, `RemootioApiCrypto` (exposed to test project via InternalsVisibleTo)
  - Do NOT add Blazor/UI dependencies here — it must remain a pure .NET library.

- **`GateMonitor.Blazor`** owns the UI: Razor components, wwwroot static assets, and `Program.cs`.
  - Consumes `IRemootioService` via DI injection — never references `RemootioDevice` or `RemootioApiCrypto` directly.
  - Config is read from `wwwroot/appsettings.json` (public, no secrets at rest).

- **`GateMonitor.AppHost`** is the Aspire orchestrator for local development only.
  - References `GateMonitor.Blazor` as an Aspire project resource.
  - No business logic here — only resource declarations.

## Blazor Component Patterns

- Components use `@inject IRemootioService` and `@inject IConfiguration` — no direct `RemootioService` references.
- Subscribe to `GateStateChanged` / `ConnectionChanged` events in `OnInitialized`, unsubscribe in `Dispose()`.
- Always call `await InvokeAsync(StateHasChanged)` from event handlers (thread-safe UI updates).
- Gate image refresh uses `System.Timers.Timer` started only when `GateImageUrl` is non-empty.

## Library Versioning

- `myNOC.Remootio` uses `GitVersion.MsBuild` with `GitHubFlow/v1` workflow.
- Targets `net8.0` and `net10.0` (net9.0 removed — EOL May 2026).
- `GeneratePackageOnBuild` is `false` — pack explicitly via `dotnet pack` or the release pipeline.

## Testing

- Library tests target the same TFMs as the library (`net8.0;net10.0`).
- Component tests target `net10.0` only.
- `bUnit.TestContext` aliased as `BunitContext` to avoid ambiguity with `MSTest.TestContext`.
- `System.Timers.Timer` is not started in tests because `GateImageUrl` is set to empty string in test setup.
- Use `NSubstitute` for mocking (not Moq) — consistent with the WeatherLink sibling project.

## Secrets

- Never commit real `DeviceIp`, `ApiSecretKey`, or `ApiAuthKey` values.
- Use `appsettings.Development.json` (git-ignored) for local overrides.
- The release pipeline uses `NUGET_PUBLISH` and `npm_token` repository secrets.
