using System.Text.Json.Serialization;

namespace myNOC.Remootio.Response
{
	internal class Action : IResponse
	{
		[JsonPropertyName("type")]
		public ActionTypes Type { get; set; }
		[JsonPropertyName("id")]
		public int Id { get; set; }
		[JsonPropertyName("success")]
		public bool Success { get; set; }
		[JsonPropertyName("state")]
		public SensorStates State { get; set; }
		[JsonPropertyName("t100ms")]
		public int T100ms { get; set; }
		[JsonPropertyName("relayTriggered")]
		public bool RelayTriggered { get; set; }
		[JsonPropertyName("errorCode")]
		public string ErrorCode { get; set; } = default!;
	}
}
