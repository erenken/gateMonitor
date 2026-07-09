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
- `@angular/*`: ^22.0.0
- `typescript`: ~6.0.0
- `rxjs`: ~7.8.0
- `zone.js`: ~0.15.0
- `ng-packagr`: ^22.0.0

## Getting Started

### Prerequisites
- Node.js 18+ 
- Angular CLI 22

### Install Angular CLI
```bash
npm install -g @angular/cli
```

### Install Dependencies
```bash
cd angular
npm install
```

### Build the Library
Before running the app, build the `remootio-angular` library:
```bash
ng build remootio-angular
```

### Run Development Server
```bash
ng serve
```

Navigate to `http://localhost:4200/`

### Build for Production
```bash
ng build --configuration production
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
npm test
```

Runs Karma test runner with ChromeHeadless for CI compatibility.

### Watch Mode (Local Development)
```bash
ng test
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
- **Workflow**: `.github/workflows/pr-build-test.yml`
- **Trigger**: Pull requests to `main`
- **Steps**: 
  - Install dependencies (`npm ci`)
  - Build app and library
  - Run tests in headless Chrome
  - Run linting

## Learn More

- [Angular Documentation](https://angular.dev)
- [RxJS Documentation](https://rxjs.dev)
- [Angular Material](https://material.angular.io)
- [Remootio API](https://github.com/remootio/remootio-api-documentation)
