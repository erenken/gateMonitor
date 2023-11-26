namespace myNOC.Remootio.Frames
{
	internal class Encrypted(IFrameData frameData, string mac) : IFrame
	{
		public FrameTypes Type => FrameTypes.ENCRYPTED;

		public IFrameData? Data => frameData;

		public string? Mac => mac;
	}
}
