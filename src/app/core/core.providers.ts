import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';

export const coreProviders = [
  provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
];
