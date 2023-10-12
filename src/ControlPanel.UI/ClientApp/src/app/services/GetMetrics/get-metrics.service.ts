import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/services/auth/auth.service';
@Injectable({
  providedIn: 'root'
})
export class GetMetricsService {

  constructor(private http: HttpClient,private auth:AuthService) { }

  getMetrics(query: string,link:string): Observable<any> {
    const prometheusUrl = `http://${link}:9090/api/v1/query?`;
    const queryParams = `query=${query}`;
    const apiUrl = `${prometheusUrl}${queryParams}`;
    return this.http.get(apiUrl, {
      headers: {
        'Authorization': `Bearer ${this.auth.getToken()}`
      }
    });
  }
}
