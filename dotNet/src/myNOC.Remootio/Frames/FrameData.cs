namespace myNOC.Remootio.Frames
{
	internal class FrameData(string iv, string payload) : IFrameData
	{
		public string IV { get; } = iv;

		public string Payload { get; } = payload;
	}
}
