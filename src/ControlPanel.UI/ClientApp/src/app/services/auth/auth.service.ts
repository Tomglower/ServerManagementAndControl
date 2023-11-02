import { Inject, Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http'
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseUrl: string = "http://localhost:5143/Auth/"
  constructor(private http: HttpClient, private router: Router) {
  }

  signUp(userObj:any){
    return this.http.post<any>(`${this.baseUrl}registration`,userObj)
  }
  
  login(loginObj:any){
    return this.http.post<any>(`${this.baseUrl}auth`,loginObj
    )
  }
  CheckToken(tokenObj: any) {
    const headers = { 'Content-Type': 'application/json' };

    return this.http.post<any>(`${this.baseUrl}authorize`, tokenObj, { headers });
  }
  storeToken(tokenValue: string){
    localStorage.setItem('token',tokenValue)
  }
  
  getToken(){
    return localStorage.getItem('token')
  }

  isLoggedIn():boolean{
    return !!localStorage.getItem('token')
  }

  signOut(){
    localStorage.clear();
    this.router.navigate(['login'])
  }
  storeId(idValue: string) {
    localStorage.setItem('UserId', idValue)
  }
  getId(idValue: string) {
    return localStorage.getItem('UserId')
  }
  autoLogin() {
    const token = this.getToken ();
    if (token) {
      if (this.CheckToken(token)) {
        this.router.navigate(['dashboard']);
        console.log("true")
      }
      else {
        console.log("false")
      }
    }
  }
  CheckTokendashboard() {
    const token = this.getToken();
    if (token) {
      if (this.CheckToken(token)) {
        console.log("true")
      }
      else {
        this.router.navigate(['login']);
        console.log("false")
      }
    }
  }

 



  
}
