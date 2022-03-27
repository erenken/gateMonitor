import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class mynocRemootioAngularService {

  private _isGateOpen: boolean = false;

  constructor() { }

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
