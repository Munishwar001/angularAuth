import { CanActivateFn ,Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthApi } from '../auth/auth-api';
import { of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { switchMap } from 'rxjs/operators';


export const guardGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthApi);
  const token = authService.getToken();
  const router = inject(Router);

  if (!token) {
    authService.logout();
    router.navigate(['/login']);
    return of(false);
  } 

  return authService.isLogin().pipe(
    map(() => true), 
    catchError(err => {
      if (err.status === 401) {
      return authService.refresh().pipe(
        switchMap((tokens:any) => {
          authService.saveToken(tokens.accessToken, tokens.refreshToken);
          return of(true); 
        }),
        catchError(refreshErr => {
          authService.logout();
          router.navigate(['/login']);
          return of(false);
        })
      );
    }
    return of(false);
  })
);
  
};
