# Copilot Instructions For GateMonitor

## Purpose
This repository contains:
- A main Angular dashboard app (gate status + image + open/close controls).
- A reusable Angular library subproject for Remootio device communication and gate control.

## Project Intent
Generate changes that keep the app simple, reliable, and local-control focused:
- Show accurate gate state quickly.
- Keep control actions explicit and safe.
- Keep device communication details inside the remootio-angular library.

## Architecture Boundaries
- UI and page behavior belong in src/.
- Remootio protocol/device communication belongs in projects/remootio-angular/.
- Shared types for gate/device state should come from remootioInterfaces.ts.

Do not move low-level protocol logic into app components.

## Coding Preferences
- Use Angular and RxJS idioms already present in the repo.
- Keep methods and changes small and readable.
- Avoid broad refactors unless requested.
- Keep naming aligned with existing gate terminology (gateState, openGate, closeGate).

## Configuration and Secrets
- Never hardcode real credentials.
- If adding config, prefer environment-based patterns.
- Preserve existing placeholder-driven setup unless asked to redesign configuration.

## Testing and Validation
When making changes, prefer validating with:
- Existing unit tests (npm test).
- Targeted checks for gate state flow and UI enable/disable logic.

## Documentation Expectations
If behavior changes, update:
- README.md (main app behavior/setup)
- projects/remootio-angular/README.md (library usage/API)

## Common Tasks
- UI improvements: update home component and templates/styles.
- Device control behavior: update remootio-angular service and related models.
- API surface changes: update public-api.ts and both READMEs.

## Out of Scope Unless Requested
- Replacing Remootio transport/protocol approach.
- Migrating framework versions.
- Large design-system or architecture rewrites.
