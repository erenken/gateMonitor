export interface IRemootioDeviceConfig {
  deviceIp: string;
  apiSecretKey: string;
  apiAuthKey: string;
  sendPingMessageEveryXMs?: number;
  autoReconnect?: boolean;
}
