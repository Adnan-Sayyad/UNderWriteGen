import { Routes } from '@angular/router';

export const POLICY_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/policy-list/policy-list').then(m => m.PolicyListPage) },
  { path: 'renewals', loadComponent: () => import('./pages/renewal-list/renewal-list').then(m => m.RenewalListPage) },
{ path: ':id', loadComponent: () => import('./pages/policy-detail/policy-detail').then(m => m.PolicyDetailPage) },
];
