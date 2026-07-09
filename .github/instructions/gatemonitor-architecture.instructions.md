---
name: GateMonitor Architecture Boundaries
description: "Use when changing Angular UI components, gate control logic, or remootio-angular integration in this repository. Enforces separation between src app code and projects/remootio-angular control code."
applyTo: "src/**, projects/remootio-angular/**"
---
# GateMonitor Architecture Rules

- Keep UI concerns in src/:
  - page components
  - templates/styles
  - view-model wiring and presentation logic

- Keep device/control concerns in projects/remootio-angular/:
  - websocket connection lifecycle
  - authentication handshake
  - frame/message parsing
  - gate command send operations
  - mapping raw state to shared interfaces

- Prefer connecting app and library via exported interfaces and observables.

- Avoid embedding real secrets in source files.

- If changing library exports or integration behavior:
  - update projects/remootio-angular/src/public-api.ts when required
  - update root README and library README when usage changes
