import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { Login } from './login/login';
import { Signup } from './signup/signup';
import {NotFound} from './not-found/not-found'
import {guardGuard} from './service/guards/guard-guard'
import { Profile } from './home/profile/profile';
import {EmailConfirmation} from './email-confirmation/email-confirmation';
import { ResetPasswordComponent } from './reset-password/reset-password'; 
import { authGuard } from './service/guards/auth-guard';
import { User } from './user/user';  
import { HomeComponent } from './home-component/home-component';
import { TwoFactorAuth } from './two-factor-auth/two-factor-auth';
import { OtpVerification } from './otp-verification/otp-verification';
 export const routes: Routes = [
  {path:"" , component:Home , canActivate :[guardGuard],
    children: [
      {path:"" , component: HomeComponent},
      { path: 'profile', component: Profile },
      {path:"user" , component:User , data: { role: 'admin' } ,canActivate: [authGuard]},
      {path:"signup" , component:Signup , data: { role: 'admin' } ,canActivate: [authGuard]},
      {path:"2FA" , component:TwoFactorAuth},
    ]
  },
  {path:"login" , component:Login},
  {path:"emailConfirm" , component:EmailConfirmation},
  {path:"otp-validation" , component:OtpVerification},
   {path:"resetPassword" , component:ResetPasswordComponent},
  {path:"**" , component:NotFound}
];

