# myNOC.Remootio

## Overview

A .NET client library for communicating with a [Remootio](https://remootio.com) smart gate and garage door controller via its WebSocket API.

This library ports the official [remootio-api-client-node](https://github.com/remootio/remootio-api-client-node) TypeScript implementation to C#, including the full AES-CBC + HMAC-SHA256 encryption protocol, the authentication challenge/response flow, and real-time state change events.

Designed to work in both ASP.NET Core server apps and Blazor WebAssembly SPAs (the browser's native WebSocket handles the device connection in WASM).

## Setup and Configuration

Add the Remootio service to your `IServiceCollection`. The same `RemootioService` instance is registered as both `IHostedService` (manages the WebSocket connection lifecycle) and `IRemootioService` (exposes gate state to your components):

```csharp
services.Configure<RemootioDeviceConfig>(configuration.GetSection("Remootio"));
services.AddSingleton<RemootioService>();
services.AddSingleton<IRemootioService>(sp => sp.GetRequiredService<RemootioService>());
services.AddHostedService(sp => sp.GetRequiredService<RemootioService>());
```

### Configuration

Bind `RemootioDeviceConfig` from your `appsettings.json`:

```json
{
  "Remootio": {
    "DeviceIp": "192.168.1.50",
    "ApiSecretKey": "<64-char hex — from the Remootio app>",
    "ApiAuthKey": "<64-char hex — from the Remootio app>",
    "SendPingMessageEveryXMs": 60000,
    "AutoReconnect": true,
    "GateImageUrl": "http://192.168.1.51/snapshot.jpg"
  }
}
```

The `ApiSecretKey` and `ApiAuthKey` are shown in the Remootio mobile app under **Remootio device → API Settings**.

> **Security note:** Store the API keys in environment variables or a secrets manager rather than in a committed `appsettings.json` file.

## Usage

### Subscribing to gate state

```csharp
var remootio = serviceProvider.GetRequiredService<IRemootioService>();

remootio.GateStateChanged += (sender, state) =>
{
    Console.WriteLine($"Gate is now: {state.Description}");  // "Open" or "Closed"
    Console.WriteLine($"IsOpen: {state.IsOpen}");
};

remootio.ConnectionChanged += (sender, connected) =>
{
    Console.WriteLine($"Connected: {connected}, Authenticated: {remootio.IsAuthenticated}");
};
```

### Sending commands

```csharp
// Only sent if the gate is currently closed (device enforces this)
remootio.OpenGate();

// Only sent if the gate is currently open (device enforces this)
remootio.CloseGate();
```

### Reading current state synchronously

```csharp
GateState? state = remootio.CurrentGateState;
bool authenticated = remootio.IsAuthenticated;
```

## Blazor Usage

In a Blazor WebAssembly component, subscribe to events in `OnInitialized` and call `InvokeAsync(StateHasChanged)` from event handlers to trigger re-renders:

```razor
@inject IRemootioService Remootio
@implements IDisposable

<div class="gate-status">@(Remootio.CurrentGateState?.Description ?? "Connecting...")</div>
<button @onclick="() => Remootio.OpenGate()"  disabled="@(Remootio.CurrentGateState?.IsOpen ?? true)">Open</button>
<button @onclick="() => Remootio.CloseGate()" disabled="@(!(Remootio.CurrentGateState?.IsOpen ?? false))">Close</button>

@code {
    protected override void OnInitialized() =>
        Remootio.GateStateChanged += OnStateChanged;

    private async void OnStateChanged(object? sender, GateState state) =>
        await InvokeAsync(StateHasChanged);

    public void Dispose() =>
        Remootio.GateStateChanged -= OnStateChanged;
}
```

## Protocol Notes

- The Remootio device listens on `ws://{DeviceIp}:8080/`
- All commands and responses after the `AUTH` frame are encrypted with **AES-128-CBC** (PKCS7 padding) using the `ApiSecretKey` (pre-auth) or `ApiSessionKey` (post-auth)
- Message integrity is verified with **HMAC-SHA256** over the raw `{"iv":"...","payload":"..."}` JSON string
- The library automatically handles the AUTH → challenge → QUERY authentication flow and re-authenticates on reconnect
