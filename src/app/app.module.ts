import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './root/app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HomeComponent } from './home/home/home.component';

import { MatCardModule } from '@angular/material/card';
import { mynocRemootioAngularService } from 'dist/mynoc-remootio-angular';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatCardModule
  ],
  providers: [
    mynocRemootioAngularService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
