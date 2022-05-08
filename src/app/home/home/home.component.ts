import { Component, OnInit } from '@angular/core';
import { mynocRemootioAngularService } from 'dist/mynoc-remootio-angular';
import { IGateState } from 'dist/mynoc-remootio-angular/lib/services/remootioInterfaces';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private gateImage: string = '';
  private gateStateField: IGateState = { isOpen: false, description: '' };

  public gateState$ = new Subject<IGateState>();
  public gateImage$ = new Subject<string>();

  constructor(private remootioService: mynocRemootioAngularService) {
    remootioService.gateState$.subscribe(gateState => {
      this.gateStateField = gateState;
      this.gateState$.next(gateState);
    });

    this.gateImage$.next(this.gateImage);
  }

  ngOnInit(): void {
    this.remootioService.connect({
      deviceIp: '',
      apiSecretKey: '',
      apiAuthKey: '',
      autoReconnect: true
    });

    setInterval(() => this.gateImage$.next(`${this.gateImage}?${Date.now()}`), 1000);
  }

  get gateState(): IGateState {
    return this.gateStateField;
  }

  get isAuthenticated(): boolean {
    return this.remootioService.isAuthenticated;
  }
}
