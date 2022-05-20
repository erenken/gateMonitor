# Remootio Angular Service

This service is a conversion of the [Remootio API Client for Node.js](https://github.com/remootio/remootio-api-client-node) module for use in Angular.  This module is a Angular Service originally written using Angular 13.3.5.  

[Remootio](https://www.remootio.com/) is a smart gate and garage door controller product.  

I created this service for use in my own project to work with my [Ghost Controls](https://ghostcontrols.com) gate.

## Build

Run `ng build remootio-angular` to build the project. The build artifacts will be stored in the `dist/` directory.

## Usage

### Step 1

Import the module into your app module

```ts
import { mynocRemootioAngularService } from 'dist/remootio-angular';

...

providers: [
  mynocRemootioAngularService
]
```

### Step 2

Inject the service into your component's TypeScript constructor and subscribe to state changes.

```ts
constructor(private remootioService: mynocRemootioAngularService) { };
```

## Example

In your component TypeScript file create a public field variable that is a `Subject<IGateState>()`

```ts
public gateState$ = new Subject<IGateState>();
```

We wire up the `gateState$` in the constructor.

```ts
constructor(private remootioService: mynocRemootioAngularService) {
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

This will start the WebSocket connection to the Remootio and authenticate.  If you don't specifiy `sendPingMessageEveryXMs` in the `IRemootioDeviceConfig` when calling the `connect` method, it will automatically be set to 60 seconds.  This keeps the connection alive, so you continue to recieve events.

You can also Open or Close the gate.  In the HTML above each button is bound to a methond on the `(click)` event.

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