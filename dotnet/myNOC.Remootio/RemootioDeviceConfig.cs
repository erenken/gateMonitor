namespace myNOC.Remootio;

public sealed class RemootioDeviceConfig
{
    public string DeviceIp { get; set; } = string.Empty;
    public string ApiSecretKey { get; set; } = string.Empty;
    public string ApiAuthKey { get; set; } = string.Empty;
    public int SendPingMessageEveryXMs { get; set; } = 60000;
    public bool AutoReconnect { get; set; } = true;
    public string GateImageUrl { get; set; } = string.Empty;
}
