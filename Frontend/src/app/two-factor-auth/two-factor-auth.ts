
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthApi } from '../service/auth/auth-api';

@Component({
  selector: 'app-two-factor-auth',
  imports: [CommonModule ,FormsModule ,ReactiveFormsModule],
  templateUrl: './two-factor-auth.html',
  styleUrl: './two-factor-auth.css'
})
export class TwoFactorAuth {
  randomCode: string = '';
  codeGenerated: boolean = false;
  tfaForm: FormGroup;

  constructor(private fb: FormBuilder ,private authService:AuthApi) {
    this.tfaForm = this.fb.group({
      enteredCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });
  }

  generateCode(): void {
    this.randomCode = Math.random().toString(36).substring(2, 8).toUpperCase();
    this.codeGenerated = true;
  }

 confirmTFA() {
  if (this.tfaForm.invalid) {
    alert('Please enter the code correctly.');
    return;
  }

  const entered = this.tfaForm.value.enteredCode.toUpperCase();

  if (entered === this.randomCode) {
    this.authService.enableTwoFactor().subscribe({
      next: (res: any) => {
        alert(res.message);
        this.tfaForm.reset();
        this.codeGenerated = false;
      },
      error: (err) => alert(err.error.message || 'Failed to enable 2FA')
    });
  } else {
    alert('Invalid code. Please try again.');
  }
}

}
