import { TestBed } from '@angular/core/testing';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { FooterComponent } from '../footer/footer.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MatDividerModule,
        RouterModule.forRoot([])
      ],
      declarations: [
        AppComponent,
        FooterComponent
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
