var builder = DistributedApplication.CreateBuilder(args);

// Angular SPA — prestart hook builds the remootio-angular library, then ng serve on port 4200
builder.AddNpmApp("gate-monitor-angular", "../../angular", "start")
    .WithHttpEndpoint(port: 4200, isProxied: false);

// Blazor WASM SPA — served by the dev server on port 5122
builder.AddProject<Projects.GateMonitor_Blazor>("gate-monitor-blazor");

builder.Build().Run();
