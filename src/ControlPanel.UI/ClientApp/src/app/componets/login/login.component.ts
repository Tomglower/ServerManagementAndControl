import { Component, OnInit } from '@angular/core';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {FormBuilder,FormGroup,Validators,FormControl} from '@angular/forms'
import { Route, Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/validateForm';
import { AuthService } from 'src/app/services/auth/auth.service';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {

  loginForm!: FormGroup
  hide = true;
constructor(private fb: FormBuilder,private auth:AuthService,private router:Router){}

ngOnInit():void{
  this.loginForm = this.fb.group({
    username:['',Validators.required],
    password:['',Validators.required]
  })
}



onLogin(){
  if(this.loginForm.valid)
  {
    console.log(this.loginForm.value)
    this.auth.login(this.loginForm.value).subscribe({
      next:(res)=>{
        alert(res.message)
        this.loginForm.reset();
        this.auth.storeToken(res.token)
        
        this.router.navigate(['dashboard'])
      },
      error:(err)=>{
        alert(err.error.message)
      }
    })
    

  }else{


     ValidateForm.validateAllFormFields(this.loginForm)
    alert("Form is invalid")

  }
}


}
