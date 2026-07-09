using GateMonitor.Blazor.Components;
using myNOC.Remootio;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<RemootioDeviceConfig>(builder.Configuration.GetSection("Remootio"));
// Same singleton instance for IRemootioService consumption and IHostedService lifecycle
builder.Services.AddSingleton<RemootioService>();
builder.Services.AddSingleton<IRemootioService>(sp => sp.GetRequiredService<RemootioService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<RemootioService>());

await builder.Build().RunAsync();
