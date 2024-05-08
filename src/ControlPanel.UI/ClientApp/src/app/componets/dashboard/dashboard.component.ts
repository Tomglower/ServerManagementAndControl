import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DashboardService } from 'src/app/services/dashboard/dashboard.service';
import { NotificationService } from 'src/app/services/notification/notification.service';
import { HttpClient } from '@angular/common/http';
import Machine from 'src/app/helpers/Machine';
import { GetMetricsService } from 'src/app/services/GetMetrics/get-metrics.service';
import { catchError, debounceTime, forkJoin, interval, map, of, throttle } from 'rxjs';
import { AuthService } from 'src/app/services/auth/auth.service';
import ValidateForm from 'src/app/helpers/validateForm';
import { MatSnackBar } from '@angular/material/snack-bar';
import { User } from 'oidc-client';
import { ChangeDetectorRef } from '@angular/core';
import {CustomMetricDialogComponent} from "../custom-metric-dialog/custom-metric-dialog.component";
import { MatDialog } from '@angular/material/dialog';
import CustomMetric from "../../helpers/CustomMetric";
import {CustomMetricAddDialogComponent} from "../custom-metric-add-dialog/custom-metric-add-dialog.component";


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],

})
export class DashboardComponent {
  dashboardForm!: FormGroup;
  Link: string = '';
  metricsData: any;
  machines: Machine[] = [];
  selectedMachine: Machine | null = null;
  userMetricInput: string = '';
  userMetricResult: number | null = null;

  customMetricName = '';
  customMetricQuery = '';
  customMetrics: CustomMetric[] = [];


  constructor(
    private fb: FormBuilder,
    private dashboardService: DashboardService,
    private notificationService: NotificationService,
    private http: HttpClient,
    private getMetricsService: GetMetricsService,
    private auth: AuthService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog

  ) { }

  ngOnInit(): void {
    this.auth.autoLogin();
    this.loadMachines();
    this.loadCustomMetrics();
    setInterval(() => {
      this.updateMachineData();
      this.loadMachines();
      this.refreshCustomMetrics();
    }, 5000);
    this.dashboardForm = this.fb.group({
      Link: ['', Validators.required],
    })
    this.dashboardForm.controls['Link'].valueChanges
      .pipe(
        debounceTime(1000),
        throttle(() => interval(1000))
      )
      .subscribe(
        (value: any) => {
          const request = { Link: value };
          this.dashboardService.Check(request).subscribe(
            (response: any) => {
              this.OpenSnackBar(response.message, 'Close');
            }

          );
        }
      );
  }

  logout() {
    this.auth.signOut();
  }

  openCustomMetricDialog() {
    this.dialog.open(CustomMetricDialogComponent, {
      data: {
        getMetric: (query: string) => {
          return this.getMetr(query); // пример использования вашего метода для получения метрики
        }
      }
    });
  }

  onButtonClick(): void {
    if (this.dashboardForm.invalid) {
      this.OpenSnackBar('Enter the machine address', 'Close')
      return;
    }

    this.dashboardService.Add(this.dashboardForm.value).subscribe(
      (response: any) => {
        this.OpenSnackBar(response.message, 'Close');
        this.loadMachines()
      },
      (err: any) => {
        this.OpenSnackBar(err.error.message,'Close');
      }
    );
  }

  OpenSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 3000,
      verticalPosition: "top",
      horizontalPosition: "center"
    });
  }

  loadMachines() {
    const userId = localStorage.getItem('UserId');

    const requestObject = { UserId: userId };

    this.http
      .post<Machine[]>('http://localhost:5143/Server/GetServer', requestObject, {
        headers: {
          'Authorization': `Bearer ${this.auth.getToken()}`
        }
      })
      .subscribe(
        (data: Machine[]) => {
          this.machines = data;
          this.checkMachineStatus();
          console.log(data)
        },
        (error) => {
          this.OpenSnackBar(`Ошибка при загрузке данных: ${error}`, 'Close');
        }
      );
  }
  checkMachineStatus() {
    for (const machine of this.machines) {
      this.http
        .post<any>('http://localhost:5143/Server/GetStatus', {link: machine.link}, {
          headers: {
            'Authorization': `Bearer ${this.auth.getToken()}`
          }
        })
        .subscribe(
          (response: any) => {
            machine.isActive = response;
            this.cdr.detectChanges();
          },
          (error) => {
            this.OpenSnackBar(`Ошибка при проверке статуса: ${error.message}`, 'Close');
          }
        );
    }
  }


  showMachineDetails(machine: Machine) {
    if (this.selectedMachine === machine) {
      this.selectedMachine = null;
    } else {
      this.selectedMachine = machine;
      this.refreshMetrics();
    }
  }

  updateMachineData() {

    if (this.selectedMachine) {
      this.http
        .post(
          'http://localhost:5143/Server/UpdateServerData',
          this.selectedMachine, {
          headers: {
            'Authorization': `Bearer ${this.auth.getToken()}`
          }
        }
        )
        .subscribe(
          (response: any) => {
            if (this.selectedMachine) {
              this.selectedMachine.data = response.data;
              this.checkMachineStatus()
              this.refreshMetrics()
            }
          }
        );
    }
  }
  deleteMachine(machine: Machine) {

    if (this.selectedMachine == machine) {
      const request = { id: this.selectedMachine.id };
      this.dashboardService.DeleteMachine(request).subscribe(
        (response: any) => {
          this.OpenSnackBar(response.Message, 'Close');
          this.loadMachines()
        },
        (err: any) => {
          this.OpenSnackBar(err.error.Message,'Close');
        }
      );
    }
  }
  getMetr(query: string) {
    if (!this.selectedMachine) {
      return of(0);
    }

    return this.dashboardService.getMetrics(this.selectedMachine.link, query).pipe(
      map((response) => {
        const result = response.data.result[0];
        if (result && result.value && result.value[1]) {
          const Metrics = parseFloat(result.value[1]);
          return Metrics;
        }
        return 0;
      }),
      catchError((error) => {
        this.OpenSnackBar(`Ошибка при обращении к API: ${error}`,'Close');
        return of(0);
      })
    );
  }
  getBotLink(): string {
    const userId = localStorage.getItem('UserId') || '';
    const startParameter = encodeURIComponent(userId);
    return `https://t.me/ControlPanelServiceBot?start=${startParameter}`;
  }

  openMachineServer(link: string) {
    const serverUrl = `http://${link}:9090/classic/graph`;
    window.open(serverUrl, '_blank');
  }

  refreshMetrics() {

    forkJoin([
      this.getMetr('node_load1'),
      this.getMetr('100 - (avg without(mode)(irate(node_cpu_seconds_total[1m])) * 100)'),
      this.getMetr('(node_memory_MemTotal_bytes - node_memory_MemAvailable_bytes) / 1024 / 1024 / 1024'),
      this.getMetr('(sum(node_filesystem_size_bytes - node_filesystem_free_bytes) by (instance)) / 1073741824'),
      this.getMetr('round(node_memory_MemTotal_bytes / 1024 / 1024 / 1024)'),
      this.getMetr('sum(node_filesystem_size_bytes) / 1024 / 1024 / 1024'),
      this.getMetr('irate(node_network_transmit_bytes_total[5m])'),
      this.getMetr('irate(node_network_receive_bytes_total[5m])'),
    ]).subscribe(
      ([load,cpuUsage, memoryUsage, diskUsage, memoryFull, diskFull,networkTransmit,networkReceive]) => {
        if (this.selectedMachine) {
          this.selectedMachine.load = load.toFixed(2);
          this.selectedMachine.cpuUsage = cpuUsage.toFixed(2);
          this.selectedMachine.memoryUsage = memoryUsage.toFixed(2);
          this.selectedMachine.diskUsage = diskUsage.toFixed(2);
          this.selectedMachine.memoryFull = memoryFull.toFixed(2);
          this.selectedMachine.diskFull = diskFull.toFixed(2);
          this.selectedMachine.networkTransmit = networkTransmit.toFixed(2);
          this.selectedMachine.networkReceive = networkReceive.toFixed(2);
        }
      },
      (error) => {
        this.OpenSnackBar(`Ошибка при получении метрик: error`,'Close');
      }
    );
  }
  addCustomMetric(): void {
    if (this.selectedMachine && this.customMetricName && this.customMetricQuery) {
      const newMetric: CustomMetric = {
        id: new Date().toISOString(), // уникальный идентификатор метрики
        name: this.customMetricName,
        query: this.customMetricQuery,
        machineId: this.selectedMachine.id,
        value: null,
      };

      this.customMetrics.push(newMetric);
      this.saveCustomMetrics(); // сохранить в Local Storage
      this.customMetricName = ''; // очистить поле ввода
      this.customMetricQuery = '';
      this.refreshCustomMetrics(); // сразу обновить значения метрик
    }
  }

  removeCustomMetric(metricId: string): void {
    this.customMetrics = this.customMetrics.filter(metric => metric.id !== metricId);
    this.saveCustomMetrics();
  }

  saveCustomMetrics(): void {
    localStorage.setItem('CustomMetrics', JSON.stringify(this.customMetrics));
  }

  loadCustomMetrics(): void {
    const savedMetrics = localStorage.getItem('CustomMetrics');
    if (savedMetrics) {
      this.customMetrics = JSON.parse(savedMetrics);
    }
  }

  refreshCustomMetrics(): void {
    const metricObservables = this.customMetrics.map(metric => {
      return this.getMetr(metric.query).pipe(
        map(value => {
          metric.value = value;
          return metric;
        })
      );
    });

    forkJoin(metricObservables).subscribe(
      () => {
        this.saveCustomMetrics(); // обновить сохраненные значения
        this.cdr.detectChanges();
      },
      (error) => {
        this.openSnackBar(`Ошибка при обновлении пользовательских метрик: ${error}`, 'Close');
      }
    );
  }

  openSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 3000,
      verticalPosition: "top",
      horizontalPosition: "center"
    });
  }

  openCustomAddMetricDialog() {
    const dialogRef = this.dialog.open(CustomMetricAddDialogComponent, {
      width: '400px',
      data: {} // можем передать данные, если нужно
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && this.selectedMachine) {
        const newMetric = {
          id: new Date().toISOString(), // уникальный ID метрики
          name: result.metricName,
          query: result.metricQuery,
          machineId: this.selectedMachine.id,
          value: null,
        };

        this.customMetrics.push(newMetric);
        this.saveCustomMetrics(); // сохранить в Local Storage
        this.refreshCustomMetrics(); // обновить метрики после добавления
      }
    });
  }
}


