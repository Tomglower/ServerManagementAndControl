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
  metricIcons = [
    { value: 'dashboard', label: 'Панель управления' },
    { value: 'cached', label: 'Загрузка' },
    { value: 'developer_board', label: 'Панель' },
    { value: 'memory', label: 'Память' },
    { value: 'sd_storage', label: 'Хранилище' },
    { value: 'analytics', label: 'Аналитика' },
    { value: 'monitor', label: 'Мониторинг' },
    { value: 'report', label: 'Отчет' },
    { value: 'api', label: 'API' }
  ]


  constructor(
    public dialogRef: MatDialogRef<CustomMetricDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private fb: FormBuilder
  ) {
    this.metricForm = this.fb.group({
      metricName: ['', Validators.required],
      metricQuery: ['', Validators.required],
      metricIcon: [''] // Добавлено новое поле для иконки
    });
  }

  onAddMetric() {
    if (this.metricForm.valid) {
      const { metricName, metricQuery, metricIcon } = this.metricForm.value;
      this.dialogRef.close({ metricName, metricQuery, metricIcon });
    }
  }

  onCancel() {
    this.dialogRef.close(null);
  }
}
