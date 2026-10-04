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
│       │   ├── services/         # RemootioService, device client
│       │   └── models/           # Interfaces and types
│       └── README.md             # Library-specific docs
└── angular.json                  # Angular workspace config
```

## Angular 22 Upgrade

This project has been upgraded from Angular 15 to Angular 22, which includes:

### Key Changes
- **@angular/build**: Replaced `@angular-devkit/build-angular` with new `@angular/build` package
- **Application builder**: Switched from `browser-esbuild` to `application` builder (new default)
- **TypeScript 6.0**: Updated with `moduleResolution: bundler`
- **Standalone components**: Added `standalone: false` to NgModule-declared components (v22 default changed)
- **Removed deprecated files**:
  - `polyfills.ts` - handled automatically by builder
  - `test.ts` - Karma builder integration improved
  - `environment.prod.ts` - no longer needed with application builder
- **Material**: Removed legacy Material imports (MatLegacyCardModule, etc.)
- **Testing**: Updated to use `RouterModule.forRoot([])` instead of deprecated `RouterTestingModule`

### Package Versions
- `@angular/*`: ^22.2.1
- `typescript`: ~6.0.3 (Angular-compatible; TypeScript 7 is excluded)
- `rxjs`: ~7.8.2
- `zone.js`: ~0.16.3
- `ng-packagr`: ^22.2.4
- `vitest` and `@vitest/browser-playwright`: ^5.0.3
- `playwright`: ^1.63.0

Karma/Jasmine and the deprecated Angular animation/dynamic-bootstrap packages have been removed. The app uses `platformBrowser` and the current Material CSS-based behavior.

## Getting Started

### Prerequisites
- Node.js 26 (also used in GitHub Actions)
- Playwright Chromium for headless tests (`npx playwright install chromium`)
- Angular CLI 22 is installed locally by `npm ci`; no global CLI is required.

### Install Dependencies
```bash
cd angular
npm ci
```

Install only at the Angular workspace root; a separate library install is not needed.

### Build the Library
Before running the app, build the `remootio-angular` library:
```bash
npm run buildService
```

### Run Development Server
```bash
npm start
```

Navigate to `http://localhost:4200/`

### Build for Production
```bash
npm run build -- --configuration production
```

## Configuration

Edit `src/app/pages/home/home.component.ts` and update the Remootio connection settings:

```typescript
const deviceIp = '192.168.1.50';
const apiSecretKey = '<64-char hex from Remootio app>';
const apiAuthKey = '<64-char hex from Remootio app>';
```

> **Security:** Do not commit real credentials. Use environment variables or a local config file (git-ignored).

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

The Angular build and tests run automatically in GitHub Actions:
- **Workflow**: [pr-build-test.yml](../.github/workflows/pr-build-test.yml)
- **Triggers**: Pull requests to `main`, manual runs and reusable release validation
- **Steps**: 
  - Install dependencies (`npm ci`)
  - Build app and library
  - Install Chromium and run Vitest tests in headless mode
  - Stamp the library with GitVersion `semVer` before building
  - Verify source, lockfile and built package versions, then dry-run npm packaging

### npm Release Version

On pushes to `main`, the [release workflow](../.github/workflows/dotnet-release.yml) updates `projects/remootio-angular/package.json` and its lockfile with GitVersion before building. `npm run verifyServiceVersion` checks the source and `dist/remootio-angular/package.json` against `PACKAGE_VERSION`; publishing stops if they differ. Only `dist/remootio-angular` is published, never the private dashboard package. The runner does not commit version changes.

Publishing uses GitHub OIDC trusted publishing, not an npm token. See [Publishing setup](../.github/PUBLISHING.md) and the [library README](projects/remootio-angular/README.md).

## Remaining Install Notices

Builds and tests run without compiler warnings, and `npm audit` is clean after the Vitest migration. A fresh install still emits upstream deprecation notices for `crypto-js@4.2.0` and the native Yuku parser/codegen bindings used by `ng-packagr` via `rolldown-plugin-dts`. There is no newer compatible release in the current dependency ranges. These notices are not suppressed or worked around with incompatible overrides. Replacing the crypto library requires separate protocol-interoperability verification.

Build-script approvals in `package.json` are pinned to the reviewed versions of Parcel watcher, esbuild, lmdb and msgpackr-extract. Review new versions before updating those approvals.

## Learn More

- [Angular Documentation](https://angular.dev)
- [RxJS Documentation](https://rxjs.dev)
- [Angular Material](https://material.angular.io)
- [Remootio API](https://github.com/remootio/remootio-api-documentation)
