namespace myNOC.Remootio.Response
{
	internal class Challenge : IResponse
	{
		public string SessionKey { get; set; } = default!;
		public int InitialActionId { get; set; } = default!;
	}
}
