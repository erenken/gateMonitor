import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { IConnectionStatus, IGateState, IRemootioDeviceConfig } from './remootioInterfaces';
import { RemootioDevice } from './remootioDevice';
import { EventTypes, isEventType, isRemootioActionResponse, ReceivedEncryptedFrameContent, RemootioActionResponse } from './remootioFrames';

@Injectable({
  providedIn: 'root'
})
export class mynocRemootioAngularService {

  private remootioDevice?: RemootioDevice;
  private authenticated: boolean = false;

  public connectionChanged$ = new Subject<boolean>();
  public messages$ = new Subject<ReceivedEncryptedFrameContent>();
  public errors$ = new Subject<string>();
  public gateState$ = new Subject<IGateState>();

  constructor() {
  }

  public connect(remootioDeviceConfig: IRemootioDeviceConfig) {
    if (this.remootioDevice && this.remootioDevice.isConnected) return;

    this.remootioDevice = new RemootioDevice(remootioDeviceConfig);

    this.remootioDevice.connectionChanged$.subscribe(connection => this.connectionChanged(connection));
    this.remootioDevice.messages$.subscribe(message => this.processMessage(message));
    this.remootioDevice.errors$.subscribe(error => this.error(error));

    var autoReconnect = true;
    if (remootioDeviceConfig.autoReconnect) autoReconnect = remootioDeviceConfig.autoReconnect;

    this.remootioDevice.connect();
  }

  public closeGate(): void {
    this.remootioDevice?.sendClose();
  }

  public openGate(): void {
    this.remootioDevice?.sendOpen();
  }

  private connectionChanged(connection: IConnectionStatus): void {
    if (connection.connected && !connection.authenticated) this.remootioDevice?.authenticate();
    if (connection.connected && connection.authenticated) this.remootioDevice?.sendQuery();

    this.authenticated = connection.authenticated;
    this.connectionChanged$.next(connection.connected);
  }

  private processMessage(message: ReceivedEncryptedFrameContent): void {
    if (isRemootioActionResponse(message)) {
      this.processRemootioActionResponse(message as RemootioActionResponse);
    } else if (isEventType(message)) {
      this.processEventTypes(message as EventTypes);
    }
  }

  private processEventTypes(message: EventTypes) {
    if (message.event.type === 'StateChange') {
      this.processState(message.event.state);
    }
  }

  private processRemootioActionResponse(message: RemootioActionResponse) {
    if (message.response.type === 'QUERY') {
      this.processState(message.response.state);
    }
  }

  private processState(sensorState: string) {
    var isOpen = sensorState === 'open';
    var gateState: IGateState = {
      isOpen: isOpen,
      description: isOpen ? "Open" : "Closed"
    };

    this.gateState$.next(gateState);
  }

  private error(error: string)
  {
    console.log(error);
    this.errors$.next(error);
  }

  public get isAuthenticated(): boolean {
    return this.authenticated;
  }
}


