import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { AuthApi } from '../service/auth/auth-api';
@Component({
  selector: 'app-email-confirmation',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.css'
})
export class EmailConfirmation {

  forgotForm: FormGroup;
  successMsg: string = '';
  errorMsg: string = '';
  loading = false;

  constructor(private fb: FormBuilder , private authService :AuthApi) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get email() {
    return this.forgotForm.get('email');
  }
  onSubmit() {
    this.successMsg = '';   
    this.errorMsg = '';

    if (this.forgotForm.invalid) {
      this.errorMsg = 'Please enter a valid email address';
      return;
    }

    this.loading = true;
    this.authService.forgotPassword(this.forgotForm.value).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMsg = res.message || 'Reset link sent! Please check your email.';
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = err.error?.message || 'Something went wrong. Try again.';
      }
    });

  }
}
