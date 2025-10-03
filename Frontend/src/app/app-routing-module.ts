import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { Login } from './login/login';
import { Signup } from './signup/signup';
import {NotFound} from './not-found/not-found'
import {guardGuard} from './service/guards/guard-guard'
import { Profile } from './home/profile/profile';
import {EmailConfirmation} from './email-confirmation/email-confirmation';

 export const routes: Routes = [
  {path:"" , component:Home , canActivate :[guardGuard],
    children: [
      { path: 'profile', component: Profile },
    ]
  },
  {path:"login" , component:Login},
  {path:"emailConfirm" , component:EmailConfirmation},
  {path:"signup" , component:Signup},
  {path:"**" , component:NotFound}
];

