---
name: Remootio Control Library Mode
description: "Use when updating the remootio-angular subproject control layer, including connection/auth flow, message parsing, gate state mapping, public interfaces, and API behavior."
argument-hint: "Describe the remootio library/control change you want"
agent: "GateMonitor Specialist"
---
Implement the requested change in projects/remootio-angular/.

Requirements:
- Keep low-level control and protocol logic in the library.
- Preserve observable-first integration patterns for Angular consumers.
- Minimize breaking API changes.
- If public interfaces or exported symbols change, update public-api and docs.

After edits:
- Run relevant tests/build checks when practical.
- Summarize API or behavior changes and migration notes if needed.
