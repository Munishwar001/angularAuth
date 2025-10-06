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

 export const routes: Routes = [
  {path:"" , component:Home , canActivate :[guardGuard],
    children: [
      { path: 'profile', component: Profile },
    ]
  },
  {path:"login" , component:Login},
  {path:"signup" , component:Signup},
  {path:"emailConfirm" , component:EmailConfirmation},
   {path:"resetPassword" , component:ResetPasswordComponent},
  {path:"**" , component:NotFound}
];

