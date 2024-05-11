import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { AuthService } from 'src/app/services/auth/auth.service';
import Machine from 'src/app/helpers/Machine';
import {forkJoin, interval, of} from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import CustomMetric from "../../helpers/CustomMetric";
import {NotificationSetting} from "../../helpers/NotificationSetting";


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
    'Использование CPU',
    'Использование памяти',
    'Использование диска',
    'Передача данных по сети',
    'Получение данных по сети'
  ];

  notificationSettings: NotificationSetting[] = [];
  machines: Machine[] = [];
  customMetrics: CustomMetric[] = [];
  constructor(
    private snackBar: MatSnackBar,
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadMachines();
    this.loadNotificationSettings();
    this.loadCustomMetrics();
    this.startMonitoring();
  }

  getAvailableMetrics(): string[] {
    const baseMetrics = [
      'Использование CPU',
      'Использование памяти',
      'Использование диска',
      'Передача данных по сети',
      'Получение данных по сети'
    ];

    const customMetricNames = this.customMetrics.map(metric => metric.name);

    return [...baseMetrics, ...customMetricNames];
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

  loadCustomMetrics(): void {
    const savedMetrics = localStorage.getItem('CustomMetrics');
    if (savedMetrics) {
      this.customMetrics = JSON.parse(savedMetrics);
    }
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

  logout() {
    this.authService.signOut();
  }
  getBotLink(): string {
    const userId = localStorage.getItem('UserId') || '';
    const startParameter = encodeURIComponent(userId);
    return `https://t.me/ControlPanelServiceBot?start=${startParameter}`;
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
    // Сначала ищем в пользовательских метриках
    const customMetric = this.customMetrics.find(m => m.name === metric);

    if (customMetric) {
      return this.http
        .post<any>('http://localhost:5143/Prometheus/GetMetricsPrometheus', { link: machineLink, query: customMetric.query }, {
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

    // Если метрика не найдена в пользовательских, используем стандартные
    return this.http
      .post<any>('http://localhost:5143/Prometheus/GetMetricsPrometheus', { link: machineLink, query: this.getStandardMetricQuery(metric) }, {
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

  getStandardMetricQuery(metric: string): string {
    switch (metric) {
      case 'Использование CPU':
        return '100 - (avg without(mode)(irate(node_cpu_seconds_total[1m])) * 100)';
      case 'Использование памяти':
        return '(node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / 1024 / 1024 / 1024';
      case 'Использование диска':
        return '(sum(node_filesystem_size_bytes - node_filesystem_free_bytes) by (instance)) / 1073741824';
      case 'Передача данных по сети':
        return 'irate(node_network_transmit_bytes_total[5m])';
      case 'Получение данных по сети':
        return 'irate(node_network_receive_bytes_total[5m])';
      default:
        return ''; // Если метрика неизвестна
    }
  }


  sendTelegramNotification(machine: Machine, metric: string, threshold: number, actualValue: number) {
    const message = `Уведомление: Метрика '${metric}' на машине '${machine.link}' превысила пороговое значение (${threshold}): фактическое значение ${actualValue.toFixed(2)}`;

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
