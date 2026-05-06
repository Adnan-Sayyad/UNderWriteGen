import { Routes } from '@angular/router';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/report-detail/report-detail').then(m => m.ReportDetailPage),
  },
];
