import { Component } from '@angular/core';
import { AnimationOptions, LottieComponent } from 'ngx-lottie'
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthApi } from '../service/auth/auth-api'
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  imports: [LottieComponent, RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  loginForm: FormGroup;

  options: AnimationOptions = {
    path: 'login.json',
  };

  constructor(private authService: AuthApi, private toastr: ToastrService , private router :Router) {
    this.loginForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required, Validators.minLength(6)])
    })
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }


  showError(message: string) {
    this.toastr.error(message, 'Error');
  }


  onSubmit() {
    if (this.loginForm.valid) {
      console.log(this.loginForm.value);
      this.authService.login(this.loginForm.value).subscribe({
        next: (res) => {
          console.log(res);
          if (res.success) {
            this.authService.saveToken(res.token);
            this.showSuccess(res.message);
            this.router.navigate(['/']);
          } else {
            this.showError(res.message);
          }
        },
        error: (err) => {
          console.error("Signup failed", err.error.message);
          this.showError(err.error.message);
        }
      })
    } else {
      this.loginForm.markAllAsTouched();
    }
  }


}
