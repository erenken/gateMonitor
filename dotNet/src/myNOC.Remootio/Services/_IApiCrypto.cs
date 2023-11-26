using myNOC.Remootio.Frames;
using myNOC.Remootio.Response;

namespace myNOC.Remootio.Services
{
	internal interface IApiCrypto
	{
		Payload? Decrypt(Encrypted encryptedFrame, string? apiSessionKey = default);
	}
}
