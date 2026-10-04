import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppModule } from '../../app.module';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { RemootioAngularService } from 'remootio-angular';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(async () => {
    const mockRemootioService = {
      connect: vi.fn(),
      closeGate: vi.fn(),
      openGate: vi.fn(),
      gateState$: of({ isOpen: false, description: 'Closed' }),
      isAuthenticated: false
    };

    await TestBed.configureTestingModule({
      imports: [AppModule,
        CommonModule,
        MatCardModule,
        MatButtonModule
      ],
      providers: [
        { provide: RemootioAngularService, useValue: mockRemootioService }
      ]
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
