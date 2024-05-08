import {Component, Inject} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from "@angular/forms";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {CustomMetricDialogComponent} from "../custom-metric-dialog/custom-metric-dialog.component";

@Component({
  selector: 'app-custom-metric-add-dialog',
  templateUrl: './custom-metric-add-dialog.component.html',
  styleUrls: ['./custom-metric-add-dialog.component.css']
})
export class CustomMetricAddDialogComponent {
  metricForm: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<CustomMetricDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private fb: FormBuilder
  ) {
    this.metricForm = this.fb.group({
      metricName: ['', Validators.required],
      metricQuery: ['', Validators.required],
    });
  }

  onAddMetric() {
    if (this.metricForm.valid) {
      const { metricName, metricQuery } = this.metricForm.value;
      this.dialogRef.close({ metricName, metricQuery });
    }
  }

  onCancel() {
    this.dialogRef.close(null);
  }
}
