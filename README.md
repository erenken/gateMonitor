# GateMonitor

A local NOC dashboard for checking and controlling a driveway gate via a [Remootio](https://remootio.com) smart gate controller connected to a [Ghost Controls](https://ghostcontrols.com) gate system.

The repo ships **two equivalent front-end implementations** and **one shared .NET library**:

| Project | Stack | Description |
|---------|-------|-------------|
| `angular/` | Angular 15 + RxJS | Original dashboard — runs in the browser, connects to the Remootio device WebSocket directly |
| `dotnet/GateMonitor.Blazor` | Blazor WebAssembly (.NET 10) | Equivalent SPA — same direct WebSocket approach, orchestrated by Aspire |
| `dotnet/myNOC.Remootio` | .NET 8 / 10 class library | Remootio protocol implementation — also published as a NuGet package |

---

## Repository Structure

```
gateMonitor/
├── angular/                         # Angular dashboard app + remootio-angular NPM library
│   ├── src/                         # App UI (home component, routing)
│   └── projects/remootio-angular/   # Angular library (Remootio WebSocket client)
├── dotnet/
│   ├── GateMonitor.AppHost/         # .NET Aspire orchestrator (dev launcher)
│   ├── GateMonitor.Blazor/          # Blazor WASM dashboard
│   ├── myNOC.Remootio/              # Remootio library → NuGet: myNOC.Remootio
│   ├── tests/
│   │   ├── myNOC.Tests.Remootio/    # MSTest unit tests for the library
│   │   └── GateMonitor.Tests.Blazor/# bUnit component tests for the Blazor app
│   └── GateMonitor.slnx             # .NET solution (SLNX format)
└── .github/
    ├── workflows/
    │   ├── dotnet-build.yml         # PR: build + test (.NET & Angular)
    │   └── dotnet-release.yml       # Main: NuGet + NPM publish + tag
    └── instructions/                # Copilot architecture guidance
```

---

## Angular App

The Angular app connects directly from the browser to the Remootio device WebSocket (`ws://{deviceIp}:8080/`), authenticates, and subscribes to gate state events.

### Setup

```bash
cd angular
npm install
npm install --prefix ./projects/remootio-angular/
ng build remootio-angular
ng serve -o
```

### Configuration

Edit `angular/src/app/pages/home/home.component.ts` and replace the placeholders:

```ts
this.remootioService.connect({
  deviceIp: '{remootioDeviceIp}',
  apiSecretKey: '{apiSecretKey}',   // 64-char hex — from Remootio app
  apiAuthKey: '{apiAuthKey}',        // 64-char hex — from Remootio app
  autoReconnect: true
});
```

The gate image URL placeholder is also in that file. Keys are found in the Remootio mobile app under **Settings → WebSocket API**.

---

## Blazor WASM App

The Blazor app is a standalone WebAssembly SPA that also connects directly from the browser to the Remootio device. It uses the `myNOC.Remootio` library.

### Run with Aspire (recommended)

```bash
cd dotnet
dotnet run --project GateMonitor.AppHost
```

This launches the Aspire dashboard and starts the Blazor dev server. Open the Aspire dashboard URL to navigate to the app.

### Run standalone

```bash
cd dotnet
dotnet run --project GateMonitor.Blazor
# opens http://localhost:5122
```

### Configuration

Edit `dotnet/GateMonitor.Blazor/wwwroot/appsettings.json`:

```json
{
  "Remootio": {
    "DeviceIp": "192.168.1.50",
    "ApiSecretKey": "<64-char hex>",
    "ApiAuthKey": "<64-char hex>",
    "AutoReconnect": true,
    "GateImageUrl": "http://192.168.1.51/snapshot.jpg"
  }
}
```

> **Note:** `wwwroot/appsettings.json` is served as a public static file. Do not commit real credentials — use `appsettings.Development.json` (git-ignored) or environment-local overrides.

---

## myNOC.Remootio Library

A .NET port of the [remootio-api-client-node](https://github.com/remootio/remootio-api-client-node) library. Handles AES-CBC + HMAC-SHA256 encryption, the authentication challenge/response flow, and real-time state change events.

See [dotnet/myNOC.Remootio/README.md](dotnet/myNOC.Remootio/README.md) for full API documentation.

**Install:**
```bash
dotnet add package myNOC.Remootio
```

---

## CI/CD

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| `dotnet-build.yml` | PR → main | GitVersion, .NET build + test, Angular build |
| `dotnet-release.yml` | Push → main | GitVersion, NuGet push, NPM publish, git tag + GitHub release |

**Required secrets:** `NUGET_PUBLISH`, `npm_token`


Once the connection to Remootio is made and authenticated the web site will display a bar indicating if the gate is open or closed and 2 buttons.  One to Open and one to Close the gate.

```html
<mat-card *ngIf="isAuthenticated">
  <mat-card-title>Status</mat-card-title>
  <mat-card-content class="statusCards">
    <div class="gateStatus"
      [ngClass]="{'opened': (gateState$ | async)?.isOpen, 'closed': !(gateState$ | async)?.isOpen}">
      {{ (gateState$ | async)?.description }}</div>
    <button (click)='closeGate()' mat-fab class="closed" [disabled]="!(gateState$ | async)?.isOpen">Close</button>
    &nbsp;
    <button (click)='openGate()' mat-fab class="opened" [disabled]="(gateState$ | async)?.isOpen">Open</button>
  </mat-card-content>
</mat-card>
```

The style sheet can be found in [styles.scss](./src/styles.scss).  The bar is <span style="color:green">Green</span> when the gate is closed or <span style="color:red">Red</span> when open.

To display your own gate image, you will need to change the `gateImage` URL.  To get the proper URL for a snap image from your camera you will need to look at your camera documentation.  I use a [UniFi G4 Instance](https://store.ui.com/collections/unifi-protect/products/camera-g4-instant), so once [enabled](https://jjj.blog/2019/12/get-snap-jpeg-from-unifi-protect-cameras/) the URL is just `https://{cameraIp}/snap.jpeg`.  Your camera will most likely be different.

```ts
private gateImage: string = "{cameraSnapUrl}";
```

This URL will get called every 1 second so it is updated in the UI.  The `Date.now()` is added so the image isn't cached by the browser.

```ts
setInterval(() => this.gateImage$.next(`${this.gateImage}?${Date.now()}`), 1000);
```

HTML to display the gate image.
```html
<img src="{{ gateImage$ | async }}" alt="Gate" class="gateImage" />
```

## Run

To run the site you will need Angular 15.0.0 CLI

```bash
npm install -g @angular/cli
```

Once that is installed you should run npm install in both the main project and **remootio-angular** project folder.

```bash
npm install
npm install --prefix .\projects\remootio-angular\
```

Once you have ran both the `npm install` commands you need to build the project first.

```bash
ng build remootio-angular
```

Once that the **remootio-angular** project is built you can build and run the main Angular project.

```bash
ng serve -o
```