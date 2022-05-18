# gateMonitor

This is an Angular 13.3.5 project I created so I could easily check the state of the gate at the end of my driveway.  I wanted to be able to easily see if the gate was open or closed and see an image from the camera at the gate before I let out my dog.  

Using a [Remootio](https://www.remootio.com/) smart gate contr5oller with my [Ghost Controls](https://ghostcontrols.com) gate.  I was able to convert their [Remootio API Client for Node.js](https://github.com/remootio/remootio-api-client-node) module for use in Angular and create a small site that gave me what I needed.

I will be running this site on a Raspberry PI with a touch screen.  The goal is to place this near the door so I can easily check and then close the gate if needed before I let out my dog.

This was my first attempt at creating my own Observable components that my site could subscribe to and display changes as they happened.  I haven't been using Angular a lot lately, so this was a way for me to experiment and also build something cool.

As part of this I also created an Angular Service [mynoc-remootio-angular](./projects/mynoc-remootio-angular/README.md) as a library, so hopefully other people may also find this useful.  I have not published it yet, but I am thinking I will after I have more of the base methods implemented.

## Website

This is a very basic site with only 1 page and route.  The main page is [home.component.html](./src/app/pages/home/home.component.html).  If you wanted to use this with your own Remootio device and camera you will need to update the [home.component.ts](./src/app/pages/home/home.component.ts) file.

You will need to change the `deviceIp`, `apiSecretKey`, and `apiAuthKey` in the `ngOnInit` method.

```ts
ngOnInit(): void {
  this.remootioService.connect({
    deviceIp: '{remootioDeviceIp}',
    apiSecretKey: '{apiSecretKey}',
    apiAuthKey: '{apiAuthKey}',
    autoReconnect: true
  });
```

You can get this information from the Remootio application on your mobile device.  It is located under Settings...Websocket API.

Once the connection to Remootio is made and authenticated the web site will display a bar indicating if the gate is open or closed and 2 buttons.  One to Open and one to Close the gate.

```html
<mat-card *ngIf="isAuthenticated">
  <mat-card-title>Status</mat-card-title>
  <mat-card-content class="statusCards">
    <div class="gateStatus"
      [ngClass]="{'opened': (gateState$ | async)?.isOpen, 'closed': !(gateState$ | async)?.isOpen}">
      {{ (gateState$ | async)?.description }}</div>
    <button (click)='closeGate()' mat-fab class="closed" [disabled]="!(gateState$ | async)?.isOpen">Close</button>
    &nbsp;
    <button (click)='openGate()' mat-fab class="opened" [disabled]="(gateState$ | async)?.isOpen">Open</button>
  </mat-card-content>
</mat-card>
```

The style sheet can be found in [styles.scss](./src/styles.scss).  The bar is <span style="color:green">Green</span> when the gate is closed or <span style="color:red">Red</span> when open.

To display your own gate image, you will need to change the `gateImage` URL.  To get the proper URL for a snap image from your camera you will need to look at your camera documentation.  I use a [UniFi G4 Instance](https://store.ui.com/collections/unifi-protect/products/camera-g4-instant), so once [enabled](https://jjj.blog/2019/12/get-snap-jpeg-from-unifi-protect-cameras/) the URL is just `https://{cameraIp}/snap.jpeg`.  Your camera will most likely be different.

```ts
private gateImage: string = "{cameraSnapUrl}";
```

This URL will get called every 1 second so it is updated in the UI.  The `Date.now()` is added so the image isn't cached by the browser.

```ts
setInterval(() => this.gateImage$.next(`${this.gateImage}?${Date.now()}`), 1000);
```

HTML to display the gate image.
```html
<img src="{{ gateImage$ | async }}" alt="Gate" class="gateImage" />
```

