import { ErrorObserver, NextObserver } from "rxjs";
import { ReceivedEncryptedFrameContent, RemootioActionResponse } from "./remootioFrames";

export interface IRemootioDeviceConfig {
  deviceIp: string;
  apiSecretKey: string;
  apiAuthKey: string;
  sendPingMessageEveryXMs?: number;
  autoReconnect?: boolean;
  openObserver?: NextObserver<Event>;
  closeObserver?: NextObserver<CloseEvent>;
  messageObserver?: NextObserver<RemootioActionResponse>;
  errorObserver?: ErrorObserver<string>;
  authenticatedObserver?: NextObserver<ReceivedEncryptedFrameContent>;
}
