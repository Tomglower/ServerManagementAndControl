import { Route, Router } from '@angular/router';
import { AuthService } from '../services/auth/auth.service';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
/*export const authGuard: CanActivateFn = (route, state) => {
  construcor
  if(this.auth)
};*/
export class AuthGuard {
  constructor(private auth:AuthService,private router:Router){

  }
  canActivate():boolean{
    if(this.auth.isLoggedIn())
    {
      return true
    }else{
      alert("Error access")
      this.router.navigate(['login'])
      return false
    }
  }
}