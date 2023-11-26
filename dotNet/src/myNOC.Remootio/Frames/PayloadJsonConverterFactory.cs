using myNOC.Remootio.Response;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace myNOC.Remootio.Frames
{
	internal class PayloadJsonConverterFactory
		: JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			if (typeToConvert == typeof(Payload))
				return true;

			if (!typeToConvert.IsGenericType)
				return false;

			if (typeToConvert.GetGenericTypeDefinition() != typeof(Payload<>))
				return false;

			return true;
		}

		public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!typeToConvert.IsGenericType)
				return new PayloadJsonConverter();

			Type responseType = typeToConvert.GetGenericArguments()[0];

			var converter = (JsonConverter)Activator.CreateInstance(typeof(PayloadTConverter<>).MakeGenericType(responseType))!;
			return converter;
		}

		private class PayloadTConverter<T> : JsonConverter<IPayload<T>> where T : IResponse
		{
			public PayloadTConverter()
			{
			}

			public override IPayload<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var payload = JsonSerializer.Deserialize(ref reader, typeToConvert) as IPayload<T>;
				return payload!;
			}

			public override void Write(Utf8JsonWriter writer, IPayload<T> value, JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value, typeof(IPayload<T>));
			}
		}

		private class PayloadJsonConverter : JsonConverter<Payload>
		{
			public override Payload? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var readerClone = reader;
				var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

				var firstPropertyName = jsonObject.EnumerateObject().FirstOrDefault().Name;
				var payloadType = Enum.Parse<PayloadTypes>(firstPropertyName, true) switch
				{
					PayloadTypes.CHALLENGE => typeof(Challenge),
					PayloadTypes.RESPONSE => typeof(Response.Action),
					_ => null
				};


				if (payloadType == null) return null;

				var genericPayload = typeof(Payload<>).MakeGenericType(payloadType);
				var deserialized = JsonSerializer.Deserialize(ref readerClone, genericPayload, options) as Payload;
				deserialized!.Type = Enum.Parse<PayloadTypes>(firstPropertyName, true);

				return deserialized;
			}

			public override void Write(Utf8JsonWriter writer, Payload value, JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
			}
		}
	}
}
