# AGENTS.md

## Project Overview
GateMonitor is an Angular application that provides a simple local dashboard for checking and controlling a driveway gate.

Primary use case:
- Show whether the gate is currently Open or Closed.
- Show a near-live camera snapshot from the gate area.
- Let the user send Open/Close commands to the gate controller.

This app is built around a Remootio smart gate controller connected to a Ghost Controls gate system.

## Workspace Structure
- Main application: src/
- Angular library subproject: projects/remootio-angular/

## What The Main App Does
The main app is a single-page Angular UI with a home screen.

Main behaviors:
- Connects to the Remootio device over WebSocket on startup.
- Authenticates using API values configured in the Home component.
- Subscribes to gate state updates through observables.
- Displays gate state and enables/disables Open/Close buttons based on current state.
- Refreshes a gate camera image URL every second with a cache-busting query value.

Key implementation point:
- Home component uses RemootioAngularService and reacts to gateState$.

## What The Subproject/Control Library Does
Subproject: projects/remootio-angular

The remootio-angular library is the control layer for the app.
It wraps low-level device communication and exposes Angular-friendly observables and methods.

Core responsibilities:
- Manage connection lifecycle to the Remootio device.
- Authenticate after connection.
- Parse incoming Remootio messages and state change events.
- Map device sensor state to a simple IGateState model.
- Expose control methods: openGate() and closeGate().

Public API surface includes:
- RemootioAngularService
- IConnectionStatus
- IRemootioDeviceConfig
- IGateState

## Runtime Flow (High Level)
1. App starts and Home component calls connect(...).
2. Service opens WebSocket to the device.
3. Service authenticates and queries current state.
4. Incoming messages are parsed into Open/Closed state.
5. UI updates from observable stream.
6. User presses Open/Close and service sends the command.

## Configuration Notes
The app currently uses placeholders in Home component for:
- deviceIp
- apiSecretKey
- apiAuthKey
- gate image URL

Do not commit real secrets.
If productionizing, move these values to environment config or secure local config.

## Build and Run
From repository root:
1. npm install
2. npm install --prefix .\\projects\\remootio-angular\\
3. ng build remootio-angular
4. ng serve -o

## Agent Guidance For This Repo
When modifying code in this repo:
- Preserve the separation between UI logic (src/) and control protocol logic (projects/remootio-angular/).
- Keep public interfaces in remootio-angular stable unless explicitly changing the API.
- Prefer observable-based state updates over polling for gate status.
- Avoid embedding sensitive values in source files.
- Update README files when behavior or setup changes.

## Useful Files
- Root app overview: README.md
- Home UI logic: src/app/pages/home/home.component.ts
- Library service: projects/remootio-angular/src/lib/services/remootio-angular.service.ts
- Library interfaces: projects/remootio-angular/src/lib/services/remootioInterfaces.ts
- Library package docs: projects/remootio-angular/README.md
