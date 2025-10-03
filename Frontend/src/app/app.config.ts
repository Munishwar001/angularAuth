import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideLottieOptions } from 'ngx-lottie';
import player from 'lottie-web';
import { provideHttpClient, withInterceptorsFromDi, withInterceptors } from '@angular/common/http';
import { provideToastr, ToastrModule } from 'ngx-toastr';
import { AuthApi } from './service/auth/auth-api';
import { routes } from './app-routing-module';
import { tokenInterceptor } from './service/token-interceptor';
export function playerFactory() {
  return player;
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptorsFromDi(),
      withInterceptors([tokenInterceptor])),
    provideLottieOptions({
      player: playerFactory,
    }),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
    }),
    AuthApi
  ]
};
