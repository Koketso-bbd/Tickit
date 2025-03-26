import { TestBed } from '@angular/core/testing';

import { TaskFilterService } from './task-filter.service';

describe('TaskFilterService', () => {
  let service: TaskFilterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TaskFilterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
