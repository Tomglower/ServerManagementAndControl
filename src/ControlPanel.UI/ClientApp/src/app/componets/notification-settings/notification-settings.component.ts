import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { AuthService } from 'src/app/services/auth/auth.service';
import Machine from 'src/app/helpers/Machine';
import {forkJoin, interval, of} from 'rxjs';
import { map, catchError } from 'rxjs/operators';

interface NotificationSetting {
  machine: Machine;
  metric: string;
  threshold: number;
  interval: number; // Новый параметр для интервала
  lastNotificationTimestamp?: number; // Для отслеживания последнего уведомления
}

@Component({
  selector: 'app-notification-settings',
  templateUrl: './notification-settings.component.html',
  styleUrls: ['./notification-settings.component.css']
})
export class NotificationSettingsComponent implements OnInit {
  selectedMachine!: Machine;
  selectedMetric!: string;
  threshold!: number;
  notificationInterval!: number; // Новый параметр интервала
  availableMetrics: string[] = [
    'CPU Usage',
    'Memory Usage',
    'Disk Usage',
    'Network Transmit',
    'Network Receive'
  ];

  notificationSettings: NotificationSetting[] = [];
  machines: Machine[] = [];

  constructor(
    private snackBar: MatSnackBar,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadMachines();
    this.loadNotificationSettings();
    this.startMonitoring();
  }

  loadMachines() {
    const userId = localStorage.getItem('UserId');
    const requestObject = { UserId: userId };

    this.http
      .post<Machine[]>('http://localhost:5143/Server/GetServer', requestObject, {
        headers: {
          'Authorization': `Bearer ${this.authService.getToken()}`
        }
      })
      .subscribe(
        (data: Machine[]) => {
          this.machines = data;
        },
        (error) => {
          this.snackBar.open(`Ошибка при загрузке данных: ${error}`, 'Close');
        }
      );
  }

  saveNotificationSetting() {
    const newSetting: NotificationSetting = {
      machine: this.selectedMachine,
      metric: this.selectedMetric,
      threshold: this.threshold,
      interval: this.notificationInterval,
    };

    this.notificationSettings.push(newSetting);
    this.saveNotificationSettings(); // Сохранить в Local Storage
    this.snackBar.open('Настройка уведомлений сохранена', 'Close', {
      duration: 3000,
      verticalPosition: 'top',
      horizontalPosition: 'center'
    });
  }

  removeNotificationSetting(setting: NotificationSetting) {
    this.notificationSettings = this.notificationSettings.filter(
      s => s !== setting
    );
    this.saveNotificationSettings();
  }

  saveNotificationSettings() {
    localStorage.setItem('NotificationSettings', JSON.stringify(this.notificationSettings));
  }

  loadNotificationSettings() {
    const savedSettings = localStorage.getItem('NotificationSettings');
    if (savedSettings) {
      this.notificationSettings = JSON.parse(savedSettings);
    }
  }

  startMonitoring() {
    interval(5000).subscribe(() => {
      this.monitorMetrics();
    });
  }

  monitorMetrics() {
    const now = new Date().getTime(); // Текущее время в миллисекундах

    this.notificationSettings.forEach(setting => {
      if (
        setting.lastNotificationTimestamp &&
        (now - setting.lastNotificationTimestamp) / 1000 < setting.interval
      ) {
        // Если прошло меньше времени, чем указанный интервал, пропустить
        return;
      }

      this.getMetric(setting.machine.link, setting.metric).pipe(
        map(value => {
          if (value >= setting.threshold) {
            this.sendTelegramNotification(setting.machine, setting.metric, setting.threshold, value);
            setting.lastNotificationTimestamp = now; // Обновить время последнего уведомления
          }
          return value;
        })
      ).subscribe(
        () => {},
        (error) => {
          this.snackBar.open(`Ошибка при мониторинге: ${error}`, 'Close');
        }
      );
    });
  }

  getMetric(machineLink: string, metric: string) {
    let query = '';
    switch (metric) {
      case 'CPU Usage':
        query = '100 - (avg without(mode)(irate(node_cpu_seconds_total[1m])) * 100)';
        break;
      case 'Memory Usage':
        query = '(node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / 1024 / 1024 / 1024';
        break;
      case 'Disk Usage':
        query = '(sum(node_filesystem_size_bytes - node_filesystem_free_bytes) by (instance)) / 1073741824';
        break;
      case 'Network Transmit':
        query = 'irate(node_network_transmit_bytes_total[5m])';
        break;
      case 'Network Receive':
        query = 'irate(node_network_receive_bytes_total[5m])';
        break;
    }

    return this.http
      .post<any>('http://localhost:5143/Prometheus/GetMetricsPrometheus', { link: machineLink, query }, {
        headers: {
          'Authorization': `Bearer ${this.authService.getToken()}`
        }
      })
      .pipe(
        map(response => {
          const result = response.data.result[0];
          if (result && result.value && result.value[1]) {
            return parseFloat(result.value[1]);
          }
          return 0;
        }),
        catchError((error) => {
          this.snackBar.open(`Ошибка при получении метрики: ${error}`, 'Close');
          return of(0);
        })
      );
  }

  sendTelegramNotification(machine: Machine, metric: string, threshold: number, actualValue: number) {
    const message = `Уведомление: Метрика '${metric}' на машине '${machine.link}' превысила пороговое значение (${threshold}): фактическое значение ${actualValue}`;

    this.http.post(
      'http://localhost:5143/Server/SendMessage',
      { Message: message },
      {
        headers: {
          'Authorization': `Bearer ${this.authService.getToken()}`
        }
      }
    ).subscribe(
      (response: any) => {
        this.snackBar.open('Уведомление отправлено в Telegram', 'Close', {
          duration: 3000,
          verticalPosition: 'top',
          horizontalPosition: 'center'
        });
      },
      (error) => {
        this.snackBar.open(`Ошибка при отправке уведомления в Telegram: ${error}`, 'Close');
      }
    );
  }
}
