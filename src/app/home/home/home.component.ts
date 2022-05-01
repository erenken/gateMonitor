import { Component, OnInit } from '@angular/core';
import { mynocRemootioAngularService } from 'dist/mynoc-remootio-angular';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  private gateImage: string = "http://192.168.4.131/snap.jpeg";

  constructor(private remootioService: mynocRemootioAngularService) { }

  ngOnInit(): void {
    this.remootioService.connect({
      deviceIp: '192.168.4.235',
      apiSecretKey: '9F4E4AC0972078EF8F1500EA891DBD84FE03548570BC99524C1FAB17C7EAFE25',
      apiAuthKey: '1CBB43A809BDED61F2B8A5C9F3B9A721883057C51688A5899E1417EB0D9CA135',
      autoReconnect: true
    });
  }

  get isGateOpen(): boolean {
    return this.remootioService.isGateOpen;
  }

  get isAuthenticated(): boolean {
    return this.remootioService.isAuthenticated;
  }

  getGateStatus(): string {
    return this.remootioService.gateStatusDescription();
  }

  get gateImageUrl(): string {
    return this.gateImage;
  }
}
