import { IRemootioDeviceConfig } from "remootio-angular";

export const environment = {
  production: true,
  remootioDeviceConfig: {
    deviceIp: '{deviceIp}',
    apiSecretKey: '{apiSecret}',
    apiAuthKey: '{apiAuth}',
    autoReconnect: true
  } as IRemootioDeviceConfig,
  gateImageUrl: '{gateImageUrl}'
};
