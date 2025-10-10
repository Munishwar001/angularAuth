
import { Component, OnInit, AfterViewInit } from '@angular/core';
import { AnimationOptions, LottieComponent } from 'ngx-lottie';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthApi } from '../service/auth/auth-api';
import { ToastrService } from 'ngx-toastr';
import { PublicClientApplication, AuthenticationResult } from '@azure/msal-browser';

declare const google: any;

@Component({
  selector: 'app-login',
  imports: [LottieComponent, RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'] 
})
export class Login implements AfterViewInit {

  loginForm: FormGroup;
  isVisible: boolean = false;
  private client: any;



  options: AnimationOptions = {
    path: 'login.json',
  };

  constructor(
    private authService: AuthApi,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.loginForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [Validators.required, Validators.minLength(6)])
    });
  }


  private msalInstance = new PublicClientApplication({
    auth: {
      clientId: 'bcec174d-8425-4e15-9d08-5293760dc0e6',
      authority: 'https://login.microsoftonline.com/2e3d185c-d9aa-4267-bab9-4cd330f953cb',
      redirectUri: 'http://localhost:52519',
    }
  });

  private msalInitialized = false;

  ngAfterViewInit(): void {

    this.loadGoogleScript().then(() => {
      this.client = google.accounts.id.initialize({
        client_id: '231552343195-eftj2qiksd8lol6njqrl5ctj9kvb44ej.apps.googleusercontent.com',
        callback: (response: any) => this.handleCredentialResponse(response),
        cancel_on_tap_outside: true
      });

      // Optional: Render the Google Sign-In button inside a container
      google.accounts.id.renderButton(
        document.getElementById('google-signin-button'),
        { theme: 'outline', size: 'large', width: '100%' }
      );
    });
  }

  loadGoogleScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (document.getElementById('google-js')) {
        resolve();
        return;
      }
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.id = 'google-js';
      script.onload = () => resolve();
      script.onerror = () => reject();
      document.body.appendChild(script);
    });
  }

  showSuccess(message: string) {
    this.toastr.success(message, 'Success');
  }

  showError(message: string) {
    this.toastr.error(message, 'Error');
  }

  isMicrosoftLoginInProgress = false;
  async microsoftSignIn() {
    try {
      if (this.isMicrosoftLoginInProgress) return;
      if (!this.msalInitialized) {
        await this.msalInstance.initialize();
        this.msalInitialized = true;
      }

      this.isMicrosoftLoginInProgress = true;

      const loginResponse: AuthenticationResult = await this.msalInstance.loginPopup({
        scopes: ['user.read', 'email', 'openid', 'profile'],
        prompt: 'select_account'
      });

      const idToken = loginResponse.idToken;
      console.log('Microsoft ID Token:', idToken);

      this.authService.microsoftLogin({ idToken }).subscribe({
        next: (res: any) => {
          if (res.success) {
            if (res.isTwoFactorRequired) {
              this.router.navigate(['/otp-validation'], { queryParams: { xid: res.email } });
              return;
            }
            this.authService.saveToken(res.token, res.refreshToken);
            this.toastr.success(res.message, 'Success');
            this.router.navigate(['/']);
          } else {
            this.toastr.error(res.message, 'Error');
          }
        },
        error: (err) => {

          this.toastr.error(err.error.message || 'Login failed', 'Error');
        }
      });

    } catch (error: any) {
      console.error('Microsoft login failed:', error);
      if (error.errorCode === 'user_cancelled' || error.errorCode === 'popup_window_closed') {
        this.toastr.info('Login popup was closed. Please try again.');
      } else if (error.errorCode === 'interaction_in_progress') {
        this.toastr.warning('Another login is in progress. Please wait.');
      } else {
        this.toastr.error('Microsoft sign-in failed', 'Error');
      }
    } finally {
      this.isMicrosoftLoginInProgress = false;
    }
  }


  onSubmit() {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (res: any) => {
          if (res.success) {
            if (res.isTwoFactorRequired) {
              this.router.navigate(['/otp-validation'], { queryParams: { xid: res.email } });
              return;
            }
            this.authService.saveToken(res.token, res.refreshToken);
            this.showSuccess(res.message);
            this.router.navigate(['/']);
          } else {
            this.showError(res.message);
          }
        },
        error: (err) => this.showError(err.error.message)
      });
    } else {
      this.loginForm.markAllAsTouched();
    }
  }


  googleSignIn() {
    const googleBtn = document.querySelector('#google-signin-button div[role="button"]') as HTMLElement;
    if (googleBtn) {
      googleBtn.click();
    } else {
      console.error('Google button not found');
    }
  }

  handleCredentialResponse(response: any) {
    console.log('Google ID Token:', response.credential);
    const idToken = response.credential;
    this.authService.googleLogin({ idToken }).subscribe({
      next: (res: any) => {
        if (res.success) {
          if (res.isTwoFactorRequired) {
            this.router.navigate(['/otp-validation'], { queryParams: { xid: res.email } });
            return;
          }
          this.authService.saveToken(res.token, res.refreshToken);
          this.toastr.success(res.message, 'Success');
          this.router.navigate(['/']);
        } else {
          this.toastr.error(res.message, 'Error');
        }
      },
      error: (err) => {
        this.toastr.error(err.error.message || 'Login failed', 'Error');
      }
    });
  }
}
