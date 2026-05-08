import { Routes } from '@angular/router';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardPage),
  },
  // Specific named routes MUST come before :id to avoid matching as a GUID
  {
    path: 'hit-ratio',
    loadComponent: () => import('./pages/hit-ratio/hit-ratio-page').then(m => m.HitRatioPage),
  },
  {
    path: 'premium',
    loadComponent: () => import('./pages/premium/premium-page').then(m => m.PremiumPage),
  },
  {
    path: 'referrals',
    loadComponent: () => import('./pages/referrals/referrals-page').then(m => m.ReferralsPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/report-detail/report-detail').then(m => m.ReportDetailPage),
  },
];
