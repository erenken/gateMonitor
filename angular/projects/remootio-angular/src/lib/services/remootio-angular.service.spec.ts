import { TestBed } from '@angular/core/testing';

import { RemootioAngularService } from './remootio-angular.service';

describe('MynocRemootioAngularService', () => {
  let service: RemootioAngularService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RemootioAngularService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
