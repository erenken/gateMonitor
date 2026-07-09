# Copilot Instructions For GateMonitor

## Purpose

This repository contains two equivalent front-end dashboards for a Remootio-controlled driveway gate, plus a shared .NET library:

| Subtree | Stack | Description |
|---------|-------|-------------|
| `angular/` | Angular 15 + RxJS | Original dashboard + remootio-angular NPM library |
| `dotnet/GateMonitor.Blazor` | Blazor WASM (.NET 10) | Blazor equivalent dashboard |
| `dotnet/myNOC.Remootio` | .NET 8/10 library | Remootio protocol → NuGet: myNOC.Remootio |
| `dotnet/GateMonitor.AppHost` | .NET Aspire | Dev orchestrator for the Blazor app |

## Project Intent

- Show accurate gate state quickly.
- Keep control actions explicit and safe.
- Keep device communication details inside the libraries (not in UI components).
- Both front-ends connect directly from the browser to the Remootio WebSocket.

## Architecture Boundaries

**Angular (`angular/`):**
- UI and page behavior → `src/`
- Remootio protocol/device communication → `projects/remootio-angular/`
- Shared types → `remootioInterfaces.ts`

**Blazor / .NET (`dotnet/`):**
- UI and components → `GateMonitor.Blazor/Components/`
- Remootio protocol, crypto, WebSocket → `myNOC.Remootio/` library only
- `IRemootioService` is the only public interface components should use
- `GateMonitor.AppHost` → Aspire orchestration only, no business logic

Do not move low-level protocol logic into UI components in either codebase.

## Coding Preferences

- Angular: use Angular and RxJS idioms already present in the repo.
- Blazor: use `await InvokeAsync(StateHasChanged)` for thread-safe UI updates; dispose event subscriptions.
- Keep methods small and readable.
- Avoid broad refactors unless requested.
- Keep naming aligned with gate terminology: `gateState`, `openGate`, `closeGate`, `IsOpen`, `Description`.

## Configuration and Secrets

- Never hardcode real credentials (`deviceIp`, `apiSecretKey`, `apiAuthKey`).
- Angular: placeholders in `home.component.ts`.
- Blazor: placeholders in `wwwroot/appsettings.json`; use `appsettings.Development.json` for real values locally.

## Testing and Validation

- Angular: `npm test` in `angular/`
- .NET: `dotnet test dotnet/GateMonitor.slnx`
- Both run automatically in `dotnet-build.yml` on PRs.

## Documentation Expectations

If behavior changes, update:
- `README.md` (repo overview)
- `angular/projects/remootio-angular/README.md` (Angular library)
- `dotnet/myNOC.Remootio/README.md` (.NET library)

## Agent Guidance

- **Angular UI / remootio-angular**: use the **GateMonitor Specialist** agent.
- **Blazor / myNOC.Remootio / Aspire / CI pipelines**: use the **GateMonitor .NET Specialist** agent.

## Out of Scope Unless Requested

- Replacing Remootio transport/protocol approach.
- Migrating framework versions.
- Large design-system or architecture rewrites.

