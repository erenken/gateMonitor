import { Component, OnInit } from '@angular/core';
import { RemootioAngularService, IGateState } from 'dist/remootio-angular';
import { Subject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private gateImage: string = environment.gateImageUrl;

  public gateState$ = new Subject<IGateState>();
  public gateImage$ = new Subject<string>();

  constructor(private remootioService: RemootioAngularService) {
    remootioService.gateState$.subscribe(gateState => {
      this.gateState$.next(gateState);
    });

    this.gateImage$.next(this.gateImage);
  }

  ngOnInit(): void {
    this.remootioService.connect(environment.remootioDeviceConfig);

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
