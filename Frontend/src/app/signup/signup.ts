import { Component } from '@angular/core';
import { AnimationOptions, LottieComponent } from 'ngx-lottie'
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormControl, Validators, AbstractControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthApi } from '../service/auth/auth-api';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-signup',
  imports: [LottieComponent, RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup {
  signupForm: FormGroup;

  options: AnimationOptions = {
    path: 'signup.json',
  };
  onSignup() {
    console.log('Signup form submitted');
  }

  constructor(private authservice: AuthApi , private toastr: ToastrService , private router:Router) {
    this.signupForm = new FormGroup({
      fullName: new FormControl('', [Validators.required]),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required, Validators.minLength(6)]),
      confirmPassword: new FormControl('', [Validators.required, Validators.minLength(6)]),
    },
      { validators: this.passwordMatchValidator })
  }
  
  showSuccess(message:string) {
    this.toastr.success(message, 'Success');
  }


  showError(message:string) {
    this.toastr.error(message, 'Error');
  }

  passwordMatchValidator(group: AbstractControl) {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password === confirm ? null : { passwordMismatch: true };
  }


  onSubmit() {
    if (this.signupForm.valid) {

      this.authservice.signup(this.signupForm.value).subscribe({
        next: (res) => { 
          console.log("reponse of signup",res);
           if(res.success){
            this.showSuccess(res.message);
            this.signupForm.reset();
            this.router.navigate(["/login"]);
           }else{
              this.showError(res.message);
           }
        },
        error : (err)=>{
          console.error("Signup failed",err.error.message);
          this.showError(err.error.message);
        }
      }
      )
    } else {
      this.signupForm.markAllAsTouched();
    }
  }
}
