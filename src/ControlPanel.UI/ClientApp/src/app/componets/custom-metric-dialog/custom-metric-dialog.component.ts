// custom-metric-dialog.component.ts
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-custom-metric-dialog',
  template: `
    <div style="padding: 16px; text-align: center;">
      <!-- Заголовок -->
      <h2 style="margin-bottom: 16px;">Введите метрику</h2>

      <!-- Поле ввода метрики -->
      <mat-card style="margin-bottom: 16px; padding: 16px;">
        <mat-form-field class="full-width">
          <mat-label>Метрика</mat-label>
          <input matInput [(ngModel)]="userMetricInput" placeholder="Пример: node_load1">
        </mat-form-field>
      </mat-card>

      <!-- Кнопка для получения результата -->
      <button
        mat-raised-button
        color="primary"
        (click)="getCustomMetric()"
        style="margin-bottom: 16px;">
        Получить результат
      </button>

      <!-- Отображение результата -->
      <div *ngIf="userMetricResult !== null" style="margin-top: 16px;">
        <mat-card style="padding: 16px;">
          <h4>Результат:</h4>
          <span>{{ userMetricResult }}</span>
        </mat-card>
      </div>

      <!-- Кнопка для закрытия диалога -->
      <div style="margin-top: 24px; text-align: center;">
        <button
          mat-raised-button
          color="warn"
          (click)="closeDialog()"
          style="width: 100px;">
          Закрыть
        </button>
      </div>
    </div>

  `,
  styles: [
    // ваши стили
  ]
})
export class CustomMetricDialogComponent {
  userMetricInput: string = '';
  userMetricResult: number | null = null;

  constructor(
    private dialogRef: MatDialogRef<CustomMetricDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  closeDialog() {
    this.dialogRef.close();
  }

  getCustomMetric() {
    // пример вызова метода, чтобы получить результат метрики
    this.data.getMetric(this.userMetricInput).subscribe((result: number) => {
      this.userMetricResult = result;
    });
  }
}
