import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { routes } from './app.routes';
import { coreProviders } from './core/core.providers';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),          // Angular 19+ — no zone.js needed
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    ...coreProviders,
  ],
};
