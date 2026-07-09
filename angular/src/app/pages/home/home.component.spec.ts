import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { RemootioAngularService } from 'remootio-angular';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let mockRemootioService: jasmine.SpyObj<RemootioAngularService>;

  beforeEach(async () => {
    mockRemootioService = jasmine.createSpyObj('RemootioAngularService', [
      'connect',
      'closeGate',
      'openGate'
    ], {
      gateState$: of({ isOpen: false, description: 'Closed' }),
      isAuthenticated: false
    });

    await TestBed.configureTestingModule({
      declarations: [ HomeComponent ],
      imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule
      ],
      providers: [
        { provide: RemootioAngularService, useValue: mockRemootioService }
      ],
      declarations: [ HomeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
