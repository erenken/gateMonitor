// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using myNOC.Remootio;

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.local.json")
	.Build();

var deviceConfiguration = new DeviceConfiguration();
configuration.GetRequiredSection("deviceConfiguration").Bind(deviceConfiguration);
await using var device = new myNOC.Remootio.Device();

var connected = await device.Connect(deviceConfiguration);

Console.WriteLine(connected.Connected);
