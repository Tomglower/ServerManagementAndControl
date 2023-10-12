import { TestBed } from '@angular/core/testing';

import { GetMetricsService } from './get-metrics.service';

describe('GetMetricsService', () => {
  let service: GetMetricsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GetMetricsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
