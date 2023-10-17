import { Component } from '@angular/core';
import {FormBuilder,FormGroup,Validators} from '@angular/forms'
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/validateForm';
import { AuthService } from 'src/app/services/auth/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {
 
  signUpForm! :FormGroup
  hide = true;
  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private snackBar: MatSnackBar){}

  ngOnInit():void{
    this.signUpForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]] 
    });
  }
  OpenSnackBar(message: string, action: string) {
    this.snackBar.open(message, action, {
      duration: 3000,
      verticalPosition: "top",
      horizontalPosition: "center"
    });
  }

onSignUp(){
  if(this.signUpForm.valid)
  {
    this.auth.signUp(this.signUpForm.value).subscribe({
      next:(res=>{
        this.OpenSnackBar(res.message,'Close')
        this.signUpForm.reset()
        this.router.navigate(['login'])
      }),error:(err=>{
        this.OpenSnackBar(err?.error.message,'Close')
      })
    })
  }else{
    ValidateForm.validateAllFormFields(this.signUpForm)
  }
}


}
