import { TestBed } from '@angular/core/testing';

import { mynocRemootioAngularService } from './mynoc-remootio-angular.service';

describe('MynocRemootioAngularService', () => {
  let service: mynocRemootioAngularService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(mynocRemootioAngularService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
