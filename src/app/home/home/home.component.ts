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
  private gateImage: string = "http://192.168.4.131/snap.jpeg";
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
      deviceIp: '192.168.4.235',
      apiSecretKey: '9F4E4AC0972078EF8F1500EA891DBD84FE03548570BC99524C1FAB17C7EAFE25',
      apiAuthKey: '1CBB43A809BDED61F2B8A5C9F3B9A721883057C51688A5899E1417EB0D9CA135',
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
