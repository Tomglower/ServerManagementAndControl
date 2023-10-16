import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/services/auth/auth.service';
@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private baseUrl: string = "http://localhost:5143/Server/"
  private baseUrlProm: string = "http://localhost:5143/Prometheus/"
  constructor(private http:HttpClient, private auth:AuthService) { }

  Add(machObj: any) {
    console.log(this.auth.getToken())
    return this.http.post<any>(`${this.baseUrl}AddServer`, machObj, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    })
   
  }
  Check(machObj: any) {
    return this.http.post<any>(`${this.baseUrl}CheckMachine`, machObj, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    })
  }

  GetMachineList(machObj:any){
    return this.http.post<any>(`${this.baseUrl}GetServers`, machObj, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    })
    
  }
  DeleteMachine(machObj: any) {
    return this.http.post<any>(`${this.baseUrl}DeleteMachine`, machObj, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    })
  }
  getMetrics(machineLink: string, query: string): Observable<any> {
    const body = {
      link: machineLink,
      query: query,
    };
    return this.http.post(`${this.baseUrlProm}GetMetricsPrometheus`, body, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    });
  }
  
}
