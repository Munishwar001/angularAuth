import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthApi } from '../service/auth/auth-api';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-otp-verification',
  imports: [CommonModule ,ReactiveFormsModule],
  templateUrl: './otp-verification.html',
  styleUrl: './otp-verification.css'
})
export class OtpVerification {
  otpForm: FormGroup;
  isSubmitting: boolean = false;
  email: string | null = null;
  otpSent: boolean = false;

  constructor(
    private authService: AuthApi,
    private toastr: ToastrService,
    private router: Router , 
    private route:ActivatedRoute
  ) {
    this.otpForm = new FormGroup({
      otp: new FormControl('', [
        Validators.required,
        Validators.pattern('^[0-9]{6}$') 
      ])
    });
  }

  get otp() {
    return this.otpForm.get('otp');
  }
  ngOnInit() {
    const xid = this.route.snapshot.queryParamMap.get('xid'); 
    this.email = xid; 
  } 
 

  submitOtp() {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const otpValue = this.otpForm.value.otp;

   this.authService.verifyOtp({  otp: otpValue, email: this.email }).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        if (res.success) {
          this.toastr.success(res.message, 'Success');
           this.authService.saveToken(res.token, res.refreshToken);
          this.router.navigate(['/']); 
        } else {
          this.toastr.error(res.message, 'Error');
        }
      },
      error: (err:any) => {
        this.isSubmitting = false;
        this.toastr.error(err.error.message || 'OTP verification failed', 'Error');
      }
    });
  }

   resendOtp() {
    // this.authService.resendOtp().subscribe({
    //   next: (res: any) => {
    //     if (res.success) {
    //       this.toastr.success(res.message, 'OTP Sent');
    //     } else {
    //       this.toastr.error(res.message, 'Error');
    //     }
    //   },
    //   error: (err:any) => {
    //     this.toastr.error(err.error.message || 'Failed to resend OTP', 'Error');
    //   }
    // });
  }
}
