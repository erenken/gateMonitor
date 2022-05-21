/*
* Converted from remootio node library (https://github.com/remootio/remootio-api-client-node/blob/master/src/index.ts
*/

import { AnonymousSubject, Subject } from 'rxjs/internal/Subject';
import { WebSocketSubject } from 'rxjs/webSocket';
import * as apicrypto from './remootioApiCrypto';
import { IConnectionStatus, IRemootioDeviceConfig } from './remootioInterfaces';
import { EncryptedFrame, ReceivedEncryptedFrameContent, ReceivedFrames, RemootioAction, RemootioActionResponse, SentFrames } from './remootioFrames';

export class RemootioDevice extends AnonymousSubject<ReceivedFrames | SentFrames | undefined> {
  private webSocketSubject?: WebSocketSubject<ReceivedFrames | SentFrames | undefined>;
  private apiSessionKey?: string;
  private lastActionId?: number;
  private waitingForAuthenticationQueryActionResponse?: boolean;

  private pingIntervalXMs: number = 60000;
  private pingIntervalHandle?: ReturnType<typeof setInterval>;
  private pingReplyTimeoutHandle?: ReturnType<typeof setTimeout>;

  public connectionChanged$ = new Subject<IConnectionStatus>();
  public messages$ = new Subject<ReceivedEncryptedFrameContent>();
  public errors$ = new Subject<string>();

  constructor(private deviceConfig: IRemootioDeviceConfig) {
    super();

    //Input check
    let hexstringRe = /[0-9A-Fa-f]{64}/g;
    if (!hexstringRe.test(deviceConfig.apiSecretKey)) {
      console.error('ApiSecretKey must be a hexstring representing a 256bit long byteArray');
    }
    hexstringRe = /[0-9A-Fa-f]{64}/g;
    if (!hexstringRe.test(deviceConfig.apiAuthKey)) {
      console.error('ApiAuthKey must be a hexstring representing a 256bit long byteArray');
    }

    //Set config
    this.webSocketSubject = undefined;

    //Session related data - will be filled out by the code
    this.apiSessionKey = undefined; //base64 encoded
    this.lastActionId = undefined;

    if (deviceConfig.sendPingMessageEveryXMs) {
      this.pingIntervalXMs = deviceConfig.sendPingMessageEveryXMs;
    }
  }

  public connect(): void {
    //Set session data to NULL
    this.apiSessionKey = undefined;
    this.lastActionId = undefined;
    this.waitingForAuthenticationQueryActionResponse = undefined;

    //We connect to the API
    this.webSocketSubject = new WebSocketSubject<ReceivedFrames | SentFrames | undefined>({
      url: `ws://${this.deviceConfig.deviceIp}:8080/`,
      openObserver: {
        next: (event) => this.openObserver(event)
      },
      closeObserver: {
        next: (event) => this.closeObserver(event)
      }
    });

    this.webSocketSubject.subscribe({
      next: data => this.dataReceived(data as ReceivedFrames),
      error: err => console.error(err),
      complete: () => this.completed()
    });
  }

  private openObserver(event: Event)
  {
    this.apiSessionKey = undefined;
    this.lastActionId = undefined;

    this.connectionChanged$.next({
      connected: true,
      authenticated: false
    });

    var pingReplyTimeout = this.pingIntervalXMs / 2;
    this.pingIntervalHandle = setInterval(() => {
      if (this.webSocketSubject != undefined && !this.webSocketSubject.closed) {
        this.pingReplyTimeoutHandle = setTimeout(() => {
          this.errors$.next(`No response for PING message in ${pingReplyTimeout} ms. Connection is broken.`);

          if (this.webSocketSubject) {
            this.webSocketSubject?.unsubscribe();
            this.pingReplyTimeoutHandle = undefined;
          }
        }, pingReplyTimeout);
        this.sendPing();
      }
    }, this.pingIntervalXMs);
  }

  private closeObserver(event: CloseEvent)
  {
    if (!this.pingIntervalHandle) {
      clearInterval(this.pingIntervalHandle);
      this.pingIntervalHandle = undefined;
    }

    this.connectionChanged$.next({
      connected: false,
      authenticated: false
    });
  }

  private dataReceived(data: ReceivedFrames)
  {
    if (!this.pingReplyTimeoutHandle) {
      clearTimeout(this.pingReplyTimeoutHandle);
      this.pingReplyTimeoutHandle = undefined;
    }

    switch (data.type) {
      case "ERROR":
        console.log(data);
        break;

      case "ENCRYPTED":
        this.encryptedMessage(data);
        break;

      default:
        console.log(data);
        break;
    }
  }

  private completed()
  {
  }

  private encryptedMessage(data: EncryptedFrame)
  {
    const decryptedPayload = apicrypto.remootioApiDecryptEncrypedFrame(
      data,
      this.deviceConfig.apiSecretKey,
      this.deviceConfig.apiAuthKey,
      this.apiSessionKey
    );

    if (decryptedPayload != undefined) {
      if ('challenge' in decryptedPayload) {
        //If it's an auth challenge
        //It's a challenge message
        this.apiSessionKey = decryptedPayload.challenge.sessionKey; //we update the session key
        this.lastActionId = decryptedPayload.challenge.initialActionId; //and the actionId (frame counter for actions)

        this.waitingForAuthenticationQueryActionResponse = true;
        this.sendQuery();
      } else {
        this.messages$.next(decryptedPayload);
      }

      if ('response' in decryptedPayload && decryptedPayload.response.id != undefined) {
        //If we get a response to one of our actions, we incremenet the last action id
        if (this.lastActionId != undefined) {
          if (
            this.lastActionId < decryptedPayload.response.id || //But we only increment if the response.id is greater than the current counter value
            (decryptedPayload.response.id == 0 && this.lastActionId == 0x7fffffff)
          ) {
            //or when we overflow from 0x7FFFFFFF to 0
            this.lastActionId = decryptedPayload.response.id; //We update the lastActionId
          }
        } else {
          this.errors$.error('Unexpected error - lastActionId is undefined');
        }

        //if it's the response to our QUERY action sent during the authentication flow the 'authenticated' event should be emitted
        if (
          decryptedPayload.response.type == 'QUERY' &&
          this.waitingForAuthenticationQueryActionResponse == true
        ) {
          this.waitingForAuthenticationQueryActionResponse = false;
          this.connectionChanged$.next({
            connected: true,
            authenticated: true
          });
        }
      }
    } else {
      this.errors$.next('Authentication or encryption error');
    }
  }

  /**
   * Sends an arbitrary frame to the Remootio device's websocket API
   * @param {Object} frameJson - Is a javascript object that will be stringified and sent to the Remootio API. A valid frameJson example for the HELLO frame is:
   * {
   *     type:"HELLO"
   * }
   */
  sendFrame(frameJson: SentFrames): void {
    if (this.webSocketSubject != undefined && !this.webSocketSubject.closed) {
      this.webSocketSubject.next(frameJson);
      // this.emit('outgoingmessage', frameJson, undefined);
    } else {
      console.warn('The websocket client is not connected');
    }
  }

  /**
   * Sends an ENCRYPTED frame with an arbitrary payload to the Remootio device's websocket API
   * @param {Object} unencryptedPayload - Is a javascript object that will be encrypted and placed into the ENCRYPTED frame's frame.data.payload. An example for a QUERY action is:
   * {
   *     action:{
   *         type:"QUERY",
   *         lastActionId = 321
   *     }
   * } where lastActionId must be an increment modulo 0x7FFFFFFF of the last action id (you can get this using the lastActionId property of the RemootioDevice class)
   */
  sendEncryptedFrame(unencryptedPayload: RemootioAction): void {
    if (this.webSocketSubject != undefined && !this.webSocketSubject.closed) {
      if (this.apiSessionKey != undefined) {
        //Upon connecting, send the AUTH frame immediately to authenticate the session
        const encryptedFrame = apicrypto.remootioApiConstructEncrypedFrame(
          JSON.stringify(unencryptedPayload),
          this.deviceConfig.apiSecretKey,
          this.deviceConfig.apiAuthKey,
          this.apiSessionKey
        );
        this.webSocketSubject.next(encryptedFrame);
        // this.emit('outgoingmessage', encryptedFrame, unencryptedPayload);
      } else {
        console.warn('Authenticate session first to send this message');
      }
    } else {
      console.warn('The websocket client is not connected');
    }
  }

  /**
   * Handles the authentication flow. It sends an AUTH frame, and then extracts the sessionKey and initialActionId from the response, then swaps the encryption keys
   * to the sessionKey and performs a valid QUERY action to finish the authentication successfully.
   */
  authenticate(): void {
    this.sendFrame({
      type: 'AUTH'
    });
  }

  /**
   * Sends a HELLO frame to the Remootio device API. The expected response is a SERVER_HELLO frame
   */
  sendHello(): void {
    this.sendFrame({
      type: 'HELLO'
    });
  }

  /**
   * Sends a PING frame to the Remootio device API. The expected response is a PONG frame. The RemootioDevice class sends periodic PING frames automatically to keep the connection alive.
   */
  sendPing(): void {
    this.sendFrame({
      type: 'PING'
    });
  }

  /**
   * Sends a QUERY action in an ENCRYPTED frame to the Remootio device API.
   * The response ENCRYPTED frame contains the gate status (open/closed)
   */
  sendQuery(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'QUERY',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends a TRIGGER action in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the output of the Remootio device. (so it opens/closes your gate or garage door depending on how your gate or garage door opener is set up)
   */
  sendTrigger(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'TRIGGER',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends a TRIGGER_SECONDARY action in an ENCRYPTED frame to the Remootio device API.
   * The action requires you to have a Remootio 2 device with one control output configured to be a "free relay output"
   * This action triggers the free relay output of the Remootio device.
   * Only supported in API version 2 or above
   */
  sendTriggerSecondary(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'TRIGGER_SECONDARY',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends an OPEN action in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the output of the Remootio device to open the gate or garage door only if the gate or garage door is currently closed.
   * This action returns an error response if there is no gate status sensor installed.
   */
  sendOpen(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'OPEN',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends an CLOSE action in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the output of the Remootio device to close the gate or garage door only if the gate or garage door is currently open.
   * This action returns an error response if there is no gate status sensor installed.
   */
  sendClose(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'CLOSE',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends a TRIGGER action with hold active duration in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the output of the Remootio device and holds it active for the duration specified in minutes
   */
  holdTriggerOutputActive(durationMins: number): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'TRIGGER',
          duration: durationMins,
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }
  /**
   * Sends a TRIGGER_SECONDARY action with hold active duration in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the secondary output of the Remootio device and holds it active for the duration specified in minutes
   */
  holdTriggerSecondaryOutputActive(durationMins: number): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'TRIGGER_SECONDARY',
          duration: durationMins,
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends a OPEN action with hold active duration in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the open direction output of the Remootio device and holds it active for the duration specified in minutes
   */
  holdOpenOutputActive(durationMins: number): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'OPEN',
          duration: durationMins,
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends a CLOSE action with hold active duration in an ENCRYPTED frame to the Remootio device API.
   * This action triggers the close direction output of the Remootio device and holds it active for the duration specified in minutes
   */
  holdCloseOutputActive(durationMins: number): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'CLOSE',
          duration: durationMins,
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  /**
   * Sends an RESTART action in an ENCRYPTED frame to the Remootio device API.
   * This action triggers a restart of the Remootio device.
   */
  sendRestart(): void {
    if (this.lastActionId != undefined) {
      this.sendEncryptedFrame({
        action: {
          type: 'RESTART',
          id: (this.lastActionId + 1) % 0x7fffffff //set frame counter to be last frame id + 1
        }
      });
    } else {
      console.warn('Unexpected error - lastActionId is undefined');
    }
  }

  //Get method for the isConnected property
  get isConnected(): boolean {
    if (this.webSocketSubject != undefined && !this.webSocketSubject.closed) {
      return true;
    } else {
      return false;
    }
  }

  //Get method for the lastActionId property
  get theLastActionId(): number | undefined {
    return this.lastActionId;
  }

  //Get method for the isAuthenticated property
  get isAuthenticated(): boolean {
    if (this.webSocketSubject != undefined && !this.webSocketSubject.closed) {
      if (this.apiSessionKey != undefined) {
        //If the session is authenticated, the apiSessionKey must be defined
        return true;
      } else {
        return false;
      }
    } else {
      return false; //The connection cannot be authenticated if it's not even established
    }
  }
}
