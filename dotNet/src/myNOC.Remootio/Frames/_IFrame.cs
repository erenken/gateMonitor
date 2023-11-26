namespace myNOC.Remootio.Frames
{
	internal interface IFrame
	{
		FrameTypes Type { get; }
		IFrameData? Data { get; }
		string? Mac { get; }
	}
}
