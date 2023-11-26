using myNOC.Remootio.Response;
using System.Text.Json.Serialization;

namespace myNOC.Remootio.Frames
{
	internal class Payload : IPayload
	{
		public PayloadTypes Type { get; set; }
	}

	internal class Payload<T> : Payload, IPayload<T> where T : IResponse
	{
		[JsonPropertyName("response")]
		public T Response { get; set; } = default!;
	}
}
