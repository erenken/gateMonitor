import { Component, OnInit } from '@angular/core';
import { mynocRemootioAngularService, IGateState } from 'dist/remootio-angular';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private gateImage: string = "{cameraSnapUrl}";

  public gateState$ = new Subject<IGateState>();
  public gateImage$ = new Subject<string>();

  constructor(private remootioService: mynocRemootioAngularService) {
    remootioService.gateState$.subscribe(gateState => {
      this.gateState$.next(gateState);
    });

    this.gateImage$.next(this.gateImage);
  }

  ngOnInit(): void {
    this.remootioService.connect({
      deviceIp: '{remootioDeviceIp}',
      apiSecretKey: '{apiSecretKey}',
      apiAuthKey: '{apiAuthKey}',
      autoReconnect: true
    });

    setInterval(() => this.gateImage$.next(`${this.gateImage}?${Date.now()}`), 1000);
  }

  get isAuthenticated(): boolean {
    return this.remootioService.isAuthenticated;
  }

  closeGate() {
    this.remootioService.closeGate();
  }

  openGate() {
    this.remootioService.openGate();
  }
}
