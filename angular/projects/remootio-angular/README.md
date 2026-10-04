# Remootio Angular Service

<a href="https://www.npmjs.com/package/remootio-angular">
  <img src="https://img.shields.io/npm/v/remootio-angular.svg?orange=blue" />
  <img alt="downloads" src="https://img.shields.io/npm/dt/remootio-angular.svg?color=blue" target="_blank" />
</a>

This service is a conversion of the [Remootio API Client for Node.js](https://github.com/remootio/remootio-api-client-node) module for use in Angular.  This module is an Angular service targeting Angular 22.2.

[Remootio](https://www.remootio.com/) is a smart gate and garage door controller product.  

I created this service for use in my own project to work with my [Ghost Controls](https://ghostcontrols.com) gate.

## Install

```bash
npm install crypto-js --save
npm install remootio-angular --save
```

## Build and Release

From the repository root:

```bash
cd angular
npm ci
npm run buildService
npm pack ./dist/remootio-angular --dry-run
```

The library requires Angular 22.2-compatible peers (`@angular/common`, `@angular/core`, `rxjs` and `crypto-js`). The dashboard and library share the workspace install.

GitHub Actions derives the release version from GitVersion `semVer`, updates this package manifest and lockfile **before** building, and verifies the version in `dist/remootio-angular/package.json` before publishing. The checked-in version is a development baseline, not the CI release version; no automated version-bump commit is created.

To reproduce version stamping locally, run these commands from `angular/`, replacing the sample with the intended version:

```bash
PACKAGE_VERSION=1.0.0-alpha.2 npm run versionService
npm run buildService
```

Then set `PACKAGE_VERSION` to that same value and run `npm run verifyServiceVersion`. In PowerShell:

```powershell
$env:PACKAGE_VERSION = "1.0.0-alpha.2"
npm.cmd run versionService
npm.cmd run buildService
npm.cmd run verifyServiceVersion
npm.cmd pack ./dist/remootio-angular --dry-run
```

Manual `npm run deployService` also requires `PACKAGE_VERSION` and stamps, builds and verifies before publishing. Prefer GitHub Actions for trusted publishing; a local publish needs your own npm authentication.

These local commands modify the source package and lockfile; review those changes before committing. CI publishes only the built directory after verification, using GitHub OIDC trusted publishing with provenance. See [Publishing setup](../../../.github/PUBLISHING.md). Local dry runs do not publish or validate OIDC authentication.

## Usage

### Step 1

Register the service in your app module providers

```ts
import { RemootioAngularService } from 'remootio-angular';

...

providers: [
  RemootioAngularService
]
```

### Step 2

Inject the service into your component's TypeScript constructor and subscribe to state changes.

```ts
constructor(private remootioService: RemootioAngularService) { };
```

## Example

In your component TypeScript file create a public field variable that is a `Subject<IGateState>()`

```ts
public gateState$ = new Subject<IGateState>();
```

We wire up the `gateState$` in the constructor.

```ts
constructor(private remootioService: RemootioAngularService) {
  remootioService.gateState$.subscribe(gateState => {
    this.gateState$.next(gateState);
  })
};
```

This can then be used in the component to display if the gate is open or closed.

```html
<div *ngIf="isAuthenticated">
  <button (click)='closeGate()' [disabled]="!(gateState$ | async)?.isOpen">Close</button>
  &nbsp;
  <button (click)='openGate()' [disabled]="(gateState$ | async)?.isOpen">Open</button>
</div>
```

In this example if the gate is Open then the Close button will be active and Open button will be disabled.  By using the async pipe when the underlying observable `gateState$` receives data the buttons will change.

We still need to connect to the Remootio device and to do that we call the `connect` method in the `ngOnInit()` method:

```ts
ngOnInit(): void {
  this.remootioService.connect({
    deviceIp: '{remootioDeviceIp}',
    apiSecretKey: '{apiSecretKey}',
    apiAuthKey: '{apiAuthKey}',
    autoReconnect: true
  });
}
```

This will start the WebSocket connection to the Remootio and authenticate.  If you don't specifiy `sendPingMessageEveryXMs` in the `IRemootioDeviceConfig` when calling the `connect` method, it will automatically be set to 60 seconds.  This keeps the connection alive, so you continue to receive events.

You can also Open or Close the gate.  In the HTML above each button is bound to a method on the `(click)` event.

```ts
closeGate() {
  this.remootioService.closeGate();
}

openGate() {
  this.remootioService.openGate();
}
```

Each method calls its corresponding action in the `remootioService`.

The last part is the `*ngIf="isAuthenticated"` in the outside `<div>`.  This is here so the buttons don't show up unless we have an authenticated connection to the Remootio device.  You can expose this in your components TypeScipt.

```ts
get isAuthenticated(): boolean {
  return this.remootioService.isAuthenticated;
}
```
