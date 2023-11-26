using myNOC.Remootio.Response;

namespace myNOC.Remootio.Frames
{
	internal interface IPayload
	{
		PayloadTypes Type { get; set; }
	}

	internal interface IPayload<T> : IPayload where T : IResponse
	{
		T Response { get; set; }
	}
}
