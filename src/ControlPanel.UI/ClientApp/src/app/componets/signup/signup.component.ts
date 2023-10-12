import { Component } from '@angular/core';
import {FormBuilder,FormGroup,Validators} from '@angular/forms'
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
  constructor(private fb: FormBuilder,private auth:AuthService,private router:Router){}

  ngOnInit():void{
    this.signUpForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]] 
    });
  }


onSignUp(){
  if(this.signUpForm.valid)
  {
    this.auth.signUp(this.signUpForm.value).subscribe({
      next:(res=>{
        alert(res.message)
        this.signUpForm.reset()
        this.router.navigate(['login'])
      }),error:(err=>{
        alert(err?.error.message)
      })
    })
    console.log(this.signUpForm.value)
  }else{
    ValidateForm.validateAllFormFields(this.signUpForm)
  }
}


}
