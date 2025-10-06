
import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule, ValidationErrors, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthApi } from '../service/auth/auth-api';
import { CommonModule } from '@angular/common';

@Component({
  imports: [ReactiveFormsModule, CommonModule],
  selector: 'app-reset-password',
  templateUrl: './reset-password.html',
  styleUrls: ['./reset-password.css']
})
export class ResetPasswordComponent {

  constructor(private route: ActivatedRoute, private authApi: AuthApi, private router: Router) { }
  resetForm = new FormGroup({
    password: new FormControl('', [Validators.required, Validators.minLength(6), this.identityPasswordValidator]),
    confirmPassword: new FormControl('', [Validators.required])
  }, { validators: this.passwordMatchValidator });


  passwordMatchValidator(group: AbstractControl) {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password === confirm ? null : { passwordMismatch: true };
  }

  identityPasswordValidator(control: AbstractControl): ValidationErrors | null {
    const value: string = control.value || '';

    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    const hasNonAlphaNum = /[^a-zA-Z0-9]/.test(value);
    const minLength = value.length >= 6;

    const valid = hasUpperCase && hasLowerCase && hasNumber && hasNonAlphaNum && minLength;

    if (!valid) {
      return {
        identityPassword: {
          hasUpperCase,
          hasLowerCase,
          hasNumber,
          hasNonAlphaNum,
          minLength
        }
      };
    }
    return null;
  }

  onReset() {
    if (this.resetForm.valid) {
      const password = this.resetForm.get('password')?.value;
      const confirmPassword = this.resetForm.get('confirmPassword')?.value;
      const email = this.route.snapshot.queryParamMap.get('email')!;
      const token = this.route.snapshot.queryParamMap.get('token')!;
      if (password !== confirmPassword) {
        alert('Passwords do not match!');
        return;
      }

      this.authApi.resetPassword({
        email: email,
        token: token,
        newPassword: password,
        confirmPassword: confirmPassword
      }).subscribe({
        next: (res) => {
          alert('Password reset successful!')
          this.router.navigate(["/login"]);
        },
        error: err => alert('Error: ' + err.error.message)
      });

    } else {
      alert('Please fill all fields correctly.');
    }
  }
}

