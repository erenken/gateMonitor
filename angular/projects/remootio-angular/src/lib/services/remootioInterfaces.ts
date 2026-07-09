export interface IConnectionStatus {
  connected: boolean;
  authenticated: boolean;
}

export interface IRemootioDeviceConfig {
  deviceIp: string;
  apiSecretKey: string;
  apiAuthKey: string;
  sendPingMessageEveryXMs?: number;
  autoReconnect?: boolean;
}

export interface IGateState {
  isOpen: boolean
  description: string
}
