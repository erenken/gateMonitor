namespace myNOC.Remootio
{
	public class DeviceConfiguration
	{
		public string HostName { get; set; } = default!;
		public string ApiSecretKey { get; set; } = default!;
		public string ApiAuthKey { get; set; } = default!;
		public int? SendPingEveryXMs { get; set; }
		public bool AutoReconnect { get; set; } = true;

		public void Copy(DeviceConfiguration other)
		{
			this.HostName = other.HostName;
			this.ApiSecretKey = other.ApiSecretKey;
			this.ApiAuthKey = other.ApiAuthKey;
			this.SendPingEveryXMs = other.SendPingEveryXMs;
			this.AutoReconnect = other.AutoReconnect;
		}
	}
}
