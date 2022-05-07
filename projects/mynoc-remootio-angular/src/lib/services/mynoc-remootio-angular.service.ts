import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { IConnectionStatus, IGateState, IRemootioDeviceConfig } from './remootioInterfaces';
import { RemootioDevice } from './remootioDevice';
import { RemootioActionResponse } from './remootioFrames';

@Injectable({
  providedIn: 'root'
})
export class mynocRemootioAngularService {

  private remootioDevice?: RemootioDevice;
  private authenticated: boolean = false;

  public connectionChanged$ = new Subject<boolean>();
  public messages$ = new Subject<RemootioActionResponse>();
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

  private connectionChanged(connection: IConnectionStatus): void {
    if (connection.connected && !connection.authenticated) this.remootioDevice?.authenticate();
    if (connection.connected && connection.authenticated) this.remootioDevice?.sendQuery();

    this.authenticated = connection.authenticated;
    this.connectionChanged$.next(connection.connected);
  }

  private processMessage(message: RemootioActionResponse): void {
    if (message.response.type === 'QUERY') {
      var isOpen = message.response.state === 'open';
      var gateState: IGateState = {
        isOpen: isOpen,
        description: isOpen ? "Open" : "Closed"
      }

      this.gateState$.next(gateState);
    }
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
