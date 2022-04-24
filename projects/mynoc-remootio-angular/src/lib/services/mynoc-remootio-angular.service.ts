import { Injectable } from '@angular/core';
import { RemootioDevice } from './remootioDevice';
import { IRemootioDeviceConfig } from './remootioDeviceConfig';
import { ReceivedEncryptedFrameContent, ReceivedFrames } from './remootioFrames';

@Injectable({
  providedIn: 'root'
})
export class mynocRemootioAngularService {

  private isGateOpenField: boolean = false;
  private remootioDevice?: RemootioDevice;

  constructor( ) {
  }

  public connect(remootioDeviceConfig: IRemootioDeviceConfig) {
    if (this.remootioDevice && this.remootioDevice.isConnected) return;
    if (this.remootioDevice) this.remootioDevice.disconnect();

    this.remootioDevice = new RemootioDevice(
      remootioDeviceConfig.deviceIp,
      remootioDeviceConfig.apiSecretKey,
      remootioDeviceConfig.apiAuthKey,
      remootioDeviceConfig.sendPingMessageEveryXMs
    );

    var autoReconnect = true;
    if (remootioDeviceConfig.autoReconnect) autoReconnect = remootioDeviceConfig.autoReconnect;

    this.remootioDevice.connect(autoReconnect);
    // this.remootioDevice.on('incomingmessage', (frame, decryptedPayload) => this.incomingMessage(frame, decryptedPayload));
    this.remootioDevice.sendQuery();
  }

  private incomingMessage(frame: ReceivedFrames, decryptedPayload: ReceivedEncryptedFrameContent | undefined) {
    console.log(decryptedPayload);
  }

  public set isGateOpen(value: boolean) {
    this.isGateOpenField = value;
  }

  public get isGateOpen(): boolean {
    return this.isGateOpenField;
  }

  public gateStatusDescription(): string {
    if (this.isGateOpen) return "Open";
    return "Closed";
  }
}
