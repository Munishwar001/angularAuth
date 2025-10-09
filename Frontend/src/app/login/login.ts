// import { Component } from '@angular/core';
// import { AnimationOptions, LottieComponent } from 'ngx-lottie'
// import { Router, RouterLink } from '@angular/router';
// import { ReactiveFormsModule } from '@angular/forms';
// import { FormGroup, FormControl, Validators } from '@angular/forms';
// import { CommonModule } from '@angular/common';
// import { AuthApi } from '../service/auth/auth-api'
// import { ToastrService } from 'ngx-toastr';

// declare const google: any;

// @Component({
//   selector: 'app-login',
//   imports: [LottieComponent, RouterLink, ReactiveFormsModule, CommonModule],
//   templateUrl: './login.html',
//   styleUrl: './login.css'
// })
// export class Login {

//   loginForm: FormGroup;

//   options: AnimationOptions = {
//     path: 'login.json',
//   };

//   constructor(private authService: AuthApi, private toastr: ToastrService , private router :Router) {
//     this.loginForm = new FormGroup({
//       email: new FormControl('', [Validators.required, Validators.email]),
//       password: new FormControl('', [Validators.required, Validators.minLength(6)])
//     })
//   }

//   showSuccess(message: string) {
//     this.toastr.success(message, 'Success');
//   }


//   showError(message: string) {
//     this.toastr.error(message, 'Error');
//   }


//   onSubmit() {
//     if (this.loginForm.valid) {
//       console.log(this.loginForm.value);
//       this.authService.login(this.loginForm.value).subscribe({
//         next: (res:any) => {
//           console.log(res);
//           if (res.success) {
//             this.authService.saveToken(res.token ,res.refreshToken);
//             this.showSuccess(res.message);
//             this.router.navigate(['/']);
//           } else {
//             this.showError(res.message);
//           }
//         },
//         error: (err) => {
//           console.error("Signup failed", err.error.message);
//           this.showError(err.error.message);
//         }
//       })
//     } else {
//       this.loginForm.markAllAsTouched();
//     }
//   }

//  googleSignIn() {
//     google.accounts.id.initialize({
//       client_id: '231552343195-eftj2qiksd8lol6njqrl5ctj9kvb44ej.apps.googleusercontent.com',
//       callback: (response: any) => this.handleCredentialResponse(response),
//       cancel_on_tap_outside: true,
//     });

//     // Show the Google account picker (ID selector)
//     google.accounts.id.prompt();
//   }

//   handleCredentialResponse(response: any) {
//     console.log('Google ID Token:', response.credential);
    
//     // send token to backend
//     // this.authService.googleLogin({ idToken: response.credential }).subscribe({
//     //   next: (res: any) => {
//     //     if (res.success) {
//     //       this.authService.saveToken(res.token, res.refreshToken);
//     //       this.toastr.success(res.message, 'Success');
//     //       this.router.navigate(['/']);
//     //     } else {
//     //       this.toastr.error(res.message, 'Error');
//     //     }
//     //   },
//     //   error: (err) => this.toastr.error(err.error.message, 'Error')
//     // });
//   }
// }
import { Component, OnInit } from '@angular/core';
import { AnimationOptions, LottieComponent } from 'ngx-lottie';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthApi } from '../service/auth/auth-api';
import { ToastrService } from 'ngx-toastr';

declare const google: any;

@Component({
  selector: 'app-login',
  imports: [LottieComponent, RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']  // fixed typo
})
export class Login implements OnInit {

  loginForm: FormGroup;

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

  ngOnInit(): void {
    this.loadGoogleScript().then(() => {
      google.accounts.id.initialize({
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

  onSubmit() {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (res: any) => {
          if (res.success) {
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

  // googleSignIn() {
  //   google.accounts.id.prompt(); 
  // }

  handleCredentialResponse(response: any) {
    console.log('Google ID Token:', response.credential);
    const idToken = response.credential;
    this.authService.googleLogin({ idToken}).subscribe({
    next: (res: any) => {
      if (res.success) {
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
