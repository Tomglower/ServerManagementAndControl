import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomMetricAddDialogComponent } from './custom-metric-add-dialog.component';

describe('CustomMetricAddDialogComponent', () => {
  let component: CustomMetricAddDialogComponent;
  let fixture: ComponentFixture<CustomMetricAddDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CustomMetricAddDialogComponent]
    });
    fixture = TestBed.createComponent(CustomMetricAddDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
