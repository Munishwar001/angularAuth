import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthApi } from '../auth/auth-api';


export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthApi);
  const router = inject(Router);
  
    const requiredRole = route.data['role'] as string | undefined;
    const role = authService.getRoleJwt()?.toLowerCase() || null;

   if (!role) {
    return router.parseUrl('/login');
  }

  if (requiredRole && role !== requiredRole.toLowerCase()) {
    return router.parseUrl('/unauthorized');
  }
    
  return true; 
};
