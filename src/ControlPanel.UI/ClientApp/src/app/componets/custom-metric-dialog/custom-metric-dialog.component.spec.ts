import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomMetricDialogComponent } from './custom-metric-dialog.component';

describe('CustomMetricDialogComponent', () => {
  let component: CustomMetricDialogComponent;
  let fixture: ComponentFixture<CustomMetricDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CustomMetricDialogComponent]
    });
    fixture = TestBed.createComponent(CustomMetricDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
