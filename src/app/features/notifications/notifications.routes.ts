import { Routes } from '@angular/router';

export const NOTIFICATIONS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/notification-center/notification-center').then(m => m.NotificationCenterPage) },
];
