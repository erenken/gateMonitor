using myNOC.Remootio.Frames;
using myNOC.Remootio.Services;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace myNOC.Remootio.Extensions
{
	public static class IServiceCollectionExtension
	{
		public static IServiceCollection AddRemootio(this IServiceCollection services)
		{
			services.AddScoped<IDevice, Device>();
			services.AddKeyedSingleton<JsonSerializerOptions>("Payload", (services,_) =>
			{
				JsonSerializerOptions jsonSerializerOptions = new()
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				};

				jsonSerializerOptions.Converters.Add(new PayloadJsonConverterFactory());
				jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				return jsonSerializerOptions;
			});

			return services;
		}

		public static IServiceCollection AddAllScoped(this IServiceCollection services, Type serviceType)
		{
			var types = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(x => !x.IsAbstract && !x.IsInterface)
				.Where(x => serviceType.IsAssignableFrom(x));

			foreach (var type in types)
				services.AddScoped(serviceType, type);

			return services;
		}
	}
}
