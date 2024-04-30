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

  constructor(
    private fb: FormBuilder,
    private dashboardService: DashboardService,
    private notificationService: NotificationService,
    private http: HttpClient,
    private getMetricsService: GetMetricsService,
    private auth: AuthService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.auth.autoLogin();
    this.loadMachines();
    setInterval(() => {
      this.updateMachineData();
    }, 360000);
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
            console.log('API response:', response);
            machine.isActive = response;
            this.cdr.detectChanges(); // Принудительно обновляем представление
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
              this.refreshMetrics()
            }
          },
          (error) => {
            this.OpenSnackBar(`Ошибка при обновлении данных машины: ${error}`,'Close');
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
}


