import { Component, OnInit } from '@angular/core';
import { mynocRemootioAngularService } from 'dist/mynoc-remootio-angular';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  private gateImage: string = "";

  constructor(private remootioService: mynocRemootioAngularService) { }

  ngOnInit(): void {
  }

  get isGateOpen(): boolean {
    return this.remootioService.isGateOpen;
  }


  getGateStatus(): string {
    return this.remootioService.gateStatusDescription();
  }

  get gateImageUrl(): string {
    return this.gateImage;
  }
}
