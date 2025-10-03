import { HttpInterceptorFn } from '@angular/common/http';
import {inject} from "@angular/core"
import { AuthApi } from './auth/auth-api';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
   const tokenService = inject(AuthApi);
  const token = tokenService.getToken();

  const cloned = token ? req.clone({
    // headers: req.headers.set('Authorization', token)
    headers: req.headers.set('Authorization', `Bearer ${token}`)

  }) : req;
   console.log('Token attached:', token);
  return next(cloned);
  // return next(req);
};
