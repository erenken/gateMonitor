import { SentFrames, SentEcryptedFrameContent, ReceivedFrames, ReceivedEncryptedFrameContent } from './remootioFrames';

export interface RemootioDeviceEvents {
  connecting: () => void;
  connected: () => void;
  authenticated: () => void;
  disconnect: () => void;
  error: (errorMessage: string) => void;
  outgoingmessage: (frame?: SentFrames, unencryptedPayload?: SentEcryptedFrameContent) => void;
  incomingmessage: (frame: ReceivedFrames, decryptedPayload?: ReceivedEncryptedFrameContent) => void;
}

export interface RemootioConnectionEvents {
  connecting: boolean;
  connected: boolean;
  status: string;
}
