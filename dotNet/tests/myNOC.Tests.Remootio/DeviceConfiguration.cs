using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myNOC.Tests.Remootio
{
	internal static class DeviceConfiguration
	{
		private static readonly IConfigurationRoot _configuration;

		static DeviceConfiguration()
		{
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.local.json")
				.Build();
		}

		internal static myNOC.Remootio.DeviceConfiguration Get()
		{
			var deviceConfiguration = new myNOC.Remootio.DeviceConfiguration();
			_configuration.GetRequiredSection("deviceConfiguration").Bind(deviceConfiguration);
			return deviceConfiguration;
		}
	}
}
