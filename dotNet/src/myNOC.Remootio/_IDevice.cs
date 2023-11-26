namespace myNOC.Remootio
{
	public interface IDevice
	{
		public ConnectionStatus ConnectionStatus { get; }
		public Task<ConnectionStatus> Connect(DeviceConfiguration configuration);
		public Task<ConnectionStatus> Authenticate();
	}
}
