import { Injectable } from '@angular/core';
import { RemootioDevice } from './remootioDevice';
import { IRemootioDeviceConfig } from './remootioDeviceConfig';
import { ReceivedEncryptedFrameContent, ReceivedFrames, RemootioActionResponse } from './remootioFrames';

@Injectable({
  providedIn: 'root'
})
export class mynocRemootioAngularService {

  private _isGateOpen: boolean = false;
  private _isAuthenticated: boolean = false;

  private _remootioDevice?: RemootioDevice;

  constructor( ) {
  }

  public connect(remootioDeviceConfig: IRemootioDeviceConfig) {
    if (this._remootioDevice && this._remootioDevice.isConnected) return;

    this._remootioDevice = new RemootioDevice(remootioDeviceConfig);

    var autoReconnect = true;
    if (remootioDeviceConfig.autoReconnect) autoReconnect = remootioDeviceConfig.autoReconnect;

    if (!remootioDeviceConfig.openObserver) {
      remootioDeviceConfig.openObserver = {
        next: (event) => this.connected(event)
      }
    }

    if (!remootioDeviceConfig.authenticatedObserver) {
      remootioDeviceConfig.authenticatedObserver = {
        next: (event) => this.authenticated(event)
      }
    }

    if (!remootioDeviceConfig.errorObserver) {
      remootioDeviceConfig.errorObserver = {
        error: (event) => this.error(event)
      }
    }

    if (!remootioDeviceConfig.messageObserver) {
      remootioDeviceConfig.messageObserver = {
        next: (message) => this.incomingMessage(message)
      }
    }

    this._remootioDevice.connect();
  }

  public incomingMessage(message: RemootioActionResponse) {
    if (message.response.type === 'QUERY') {
      this.isGateOpen = message.response.state === 'open';
    }
  }

  public error(error: string)
  {
    console.log(error);
  }

  public connected(event: Event)
  {
    this._remootioDevice!.authenticate();
  }

  public authenticated(event: ReceivedEncryptedFrameContent)
  {
    this._isAuthenticated = true;
    this._remootioDevice!.sendQuery();
  }

  public get isAuthenticated(): boolean {
    return this._isAuthenticated;
  }

  public set isGateOpen(value: boolean) {
    this._isGateOpen = value;
  }

  public get isGateOpen(): boolean {
    return this._isGateOpen;
  }

  public gateStatusDescription(): string {
    if (this.isGateOpen) return "Open";
    return "Closed";
  }
}
