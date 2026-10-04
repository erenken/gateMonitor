# GateMonitor Angular Dashboard

Angular 22 dashboard for monitoring and controlling a Remootio-connected gate.

## Features

- Real-time gate state display (open/closed)
- Direct WebSocket connection to Remootio device from browser
- Live gate camera image refresh
- Safe open/close controls with state-aware button enabling
- Reusable `remootio-angular` NPM library for Remootio protocol

## Project Structure

```
angular/
├── src/                           # Main dashboard app
│   ├── app/
│   │   ├── pages/home/           # Home component with gate controls
│   │   ├── footer/               # Footer component
│   │   └── root/                 # App root component
│   └── environments/             # Environment configuration
├── projects/
│   └── remootio-angular/         # Reusable Angular library
│       ├── src/lib/
│       │   └── services/         # RemootioAngularService, device client, interfaces
│       └── README.md             # Library-specific docs
└── angular.json                  # Angular workspace config
```

## Getting Started

### Prerequisites

- Node.js 26 (also used in GitHub Actions)
- Angular CLI 22 is installed locally by `npm ci`; no global CLI is required.

Dependency ranges are defined in [package.json](package.json), with resolved versions in `package-lock.json`. The workspace uses Angular 22.2 and TypeScript 6.0.

### Install Dependencies

```bash
cd angular
npm ci
```

Install only at the Angular workspace root; a separate library install is not needed.

### Run Development Server

```bash
npm start
```

`npm start` builds the `remootio-angular` library through its `prestart` hook before launching the dashboard. Navigate to `http://localhost:4200/`.

After editing library code, run `npm run buildService` again so the dashboard can use the changes.

### Build for Production

```bash
npm run buildService
npm run build -- --configuration production
```

## Configuration

Edit `src/app/pages/home/home.component.ts` and update the Remootio connection settings:

```typescript
this.remootioService.connect({
  deviceIp: '192.168.1.50',
  apiSecretKey: '<64-char hex from Remootio app>',
  apiAuthKey: '<64-char hex from Remootio app>'
});
```

Find the keys in the Remootio mobile app's API settings. The browser must be able to reach the device at `ws://{deviceIp}:8080/`.

Do not commit real credentials. If you move settings to the git-ignored `src/environments/environment.local.ts`, import that configuration explicitly; the app does not load it automatically. Browser bundles and runtime configuration remain visible to users who can access the app.

The service currently accepts `autoReconnect` in its configuration interface but does not implement automatic reconnection.

### Camera Image

To display your gate camera image, update the `gateImage` URL in `home.component.ts`:

```typescript
private gateImage: string = "http://192.168.1.51/snapshot.jpg";
```

The image refreshes every second with a cache-busting timestamp.

## Testing

### Run Unit Tests

```bash
npx playwright install chromium
npm test -- --watch=false --browsers=ChromiumHeadless
```

Runs both app and library Vitest tests once in headless Chromium. On Linux CI, install browser prerequisites with `npx playwright install --with-deps chromium`. To use an existing Chrome installation, set `CHROME_BIN` to its executable path and retain `ChromiumHeadless` as the browser name.

### Watch Mode (Local Development)

```bash
npm test
```

## remootio-angular Library

The `projects/remootio-angular` folder contains a reusable Angular library that handles:

- WebSocket connection to Remootio device
- AES-CBC + HMAC-SHA256 encryption/decryption
- Authentication challenge/response
- Real-time gate state events
- Connection state management

See [projects/remootio-angular/README.md](projects/remootio-angular/README.md) for library API documentation.

## CI/CD

The [build/test workflow](../.github/workflows/pr-build-test.yml) builds the dashboard and library, runs headless Vitest tests, and validates npm packaging for pull requests to `main`, manual runs, and reusable release validation.

Pushes to `main` run the [release workflow](../.github/workflows/dotnet-release.yml), which publishes the built library. See the [publishing guide](../.github/PUBLISHING.md) for version stamping, local package verification, and GitHub OIDC setup.

## Dependency Maintenance

Build-script approvals in `package.json` are pinned to the reviewed versions of Parcel watcher, esbuild, lmdb and msgpackr-extract. Review new versions before updating those approvals.

## Learn More

- [Angular Documentation](https://angular.dev)
- [RxJS Documentation](https://rxjs.dev)
- [Angular Material](https://material.angular.io)
- [Remootio API](https://github.com/remootio/remootio-api-documentation)
