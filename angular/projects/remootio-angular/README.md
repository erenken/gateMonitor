# remootio-angular

[![npm version](https://img.shields.io/npm/v/remootio-angular.svg)](https://www.npmjs.com/package/remootio-angular)
[![npm downloads](https://img.shields.io/npm/dt/remootio-angular.svg)](https://www.npmjs.com/package/remootio-angular)

An Angular service for communicating with a [Remootio](https://www.remootio.com/) smart gate or garage door controller over its WebSocket API. It handles authentication, encrypted commands, and real-time gate state updates.

The library ports the official [Remootio API Client for Node.js](https://github.com/remootio/remootio-api-client-node) and is used by the GateMonitor dashboard with a [Ghost Controls](https://ghostcontrols.com) gate.

## Installation and Compatibility

Install in your Angular application:

```bash
npm install remootio-angular crypto-js
```

Your application must provide these peer dependencies:

| Package | Supported version range |
| --- | --- |
| `@angular/common` | `^22.2.1` |
| `@angular/core` | `^22.2.1` |
| `rxjs` | `^7.8.2` |
| `crypto-js` | `^4.2.0` |

## Usage

`RemootioAngularService` is provided in the root injector, so you can inject it directly without adding a provider. This standalone component connects to the device and displays gate controls:

```ts
import { AsyncPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RemootioAngularService } from 'remootio-angular';

@Component({
  selector: 'app-gate',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    @let state = remootio.gateState$ | async;
    @let connected = remootio.connectionChanged$ | async;

    @if (connected && remootio.isAuthenticated && state) {
      <p>Gate is {{ state.description }}</p>
      <button (click)="remootio.openGate()" [disabled]="state.isOpen">
        Open
      </button>
      <button (click)="remootio.closeGate()" [disabled]="!state.isOpen">
        Close
      </button>
    } @else {
      <p>Waiting for an authenticated connection and gate state...</p>
    }
  `
})
export class GateComponent implements OnInit {
  readonly remootio = inject(RemootioAngularService);

  ngOnInit(): void {
    this.remootio.connect({
      deviceIp: '192.168.1.50',
      apiSecretKey: '<64-character hex API secret key>',
      apiAuthKey: '<64-character hex API authentication key>'
    });
  }
}
```

Replace the placeholders with the keys from the Remootio mobile app's API settings. The browser must be able to reach the device at `ws://{deviceIp}:8080/`. Open/Close commands require a configured gate status sensor on the device.

The example binds directly to the service streams. Angular's [AsyncPipe](https://angular.dev/api/common/AsyncPipe) updates the view and unsubscribes when the component is destroyed. The streams do not replay earlier values, so subscribe before connecting; the example keeps both subscriptions outside the conditional controls.

Keep real API keys out of version control. Keys included in a browser bundle or runtime configuration are visible to users who can access the application.

## Configuration

`connect()` accepts an `IRemootioDeviceConfig`:

| Property | Description |
| --- | --- |
| `deviceIp` | Required device IP address; the library uses WebSocket port 8080. |
| `apiSecretKey` | Required 64-character hexadecimal API secret key. |
| `apiAuthKey` | Required 64-character hexadecimal API authentication key. |
| `sendPingMessageEveryXMs` | Optional keep-alive interval in milliseconds; defaults to 60000. |
| `autoReconnect` | Accepted by the interface, but automatic reconnection is not currently implemented. |

## Service API

| Member | Description |
| --- | --- |
| `connect(config)` | Starts the WebSocket connection and authentication flow, then queries gate state. |
| `gateState$` | Emits `IGateState` values with `isOpen` and `description` (`Open` or `Closed`). |
| `connectionChanged$` | Emits a connection boolean; check `isAuthenticated` separately. |
| `isAuthenticated` | Reports whether the service has an authenticated connection. |
| `openGate()` | Sends an Open command to the device. |
| `closeGate()` | Sends a Close command to the device. |
| `errors$` | Emits errors reported by the protocol client. |
| `messages$` | Emits decrypted device messages. |

## Development

For workspace builds and tests, see the [Angular dashboard README](https://github.com/erenken/gateMonitor/blob/main/angular/README.md). Maintainer instructions for version stamping, package verification, and publishing are in the [publishing guide](https://github.com/erenken/gateMonitor/blob/main/.github/PUBLISHING.md).
