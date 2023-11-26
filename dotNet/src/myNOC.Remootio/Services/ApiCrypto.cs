using myNOC.Remootio.Frames;
using myNOC.Remootio.Response;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace myNOC.Remootio.Services
{
	internal class ApiCrypto(IServiceProvider services, DeviceConfiguration deviceConfiguration) : IApiCrypto
	{
		public Payload? Decrypt(Encrypted encryptedFrame, string? apiSessionKey = default)
		{
			byte[] secretKey = default!;
			if (apiSessionKey == null)
				secretKey = Convert.FromHexString(deviceConfiguration.ApiSecretKey);
			else
				secretKey = Convert.FromBase64String(apiSessionKey);

			var apiAuthKey = Convert.FromHexString(deviceConfiguration.ApiAuthKey);

			using var sha256 = new HMACSHA256();
			sha256.Key = apiAuthKey;

			if (encryptedFrame.Data == null) return default;

			var jsonSerializerOptions = services.GetRequiredKeyedService<JsonSerializerOptions>("Payload");
			var serializedData = JsonSerializer.Serialize<IFrameData>(encryptedFrame.Data, jsonSerializerOptions);
			var computedMac = sha256.ComputeHash(Encoding.UTF8.GetBytes(serializedData));
			var base64Mac = Convert.ToBase64String(computedMac);

			//	Check to see if computed mac matches the one from the API
			if (encryptedFrame.Mac != Convert.ToBase64String(computedMac)) return default;

			var encryptedPayload = Convert.FromBase64String(encryptedFrame.Data.Payload);
			var iv = Convert.FromBase64String(encryptedFrame.Data.IV);

			using var aes = Aes.Create();
			aes.Key = secretKey;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			var decryptedPayload = aes.DecryptCbc(encryptedPayload, iv);
			Console.WriteLine(Encoding.UTF8.GetString(decryptedPayload));

			var payload = JsonSerializer.Deserialize<Payload>(decryptedPayload, jsonSerializerOptions);
			return payload;
		}
	}
}


