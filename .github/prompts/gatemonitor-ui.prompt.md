---
name: GateMonitor UI Work Mode
description: "Use when improving or refactoring the GateMonitor Angular UI in src/, especially home page layout, status display, camera image behavior, and gate control buttons."
argument-hint: "Describe the UI change you want in the main app"
agent: "GateMonitor Specialist"
---
Implement the requested UI change in the main GateMonitor app under src/.

Requirements:
- Keep existing gate-state semantics intact.
- Preserve open/close safety and button enable/disable behavior.
- Do not move protocol/device logic out of the remootio-angular library.
- Keep styles and templates consistent with current app structure.

After edits:
- Run appropriate checks or tests when practical.
- Summarize changed files and behavioral impact.
