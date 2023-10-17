import { Component, OnInit } from '@angular/core';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {FormBuilder,FormGroup,Validators,FormControl} from '@angular/forms'
import { Route, Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/validateForm';
import { AuthService } from 'src/app/services/auth/auth.service';
import { interval, throttle } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {

  loginForm!: FormGroup
  hide = true;
  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private snackBar: MatSnackBar){}

ngOnInit():void{
  this.loginForm = this.fb.group({
    username:['',Validators.required],
    password:['',Validators.required]
  })
}

  OpenSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 3000,
      verticalPosition: "top",
      horizontalPosition: "center"
    });
  }

onLogin(){
  if(this.loginForm.valid)
  {
    this.auth.login(this.loginForm.value).subscribe({
      next:(res)=>{
        this.OpenSnackBar(res.message,'Close')
        this.loginForm.reset();
        this.auth.storeToken(res.token)
        this.auth.storeId(res.id)
        this.router.navigate(['dashboard'])
      },
      error:(err)=>{
        this.OpenSnackBar(err.error.message,'Close')
      }
    })
    

  }else{
     ValidateForm.validateAllFormFields(this.loginForm)
    this.OpenSnackBar("Form is invalid",'Close')

  }
}


}
