import { Routes } from '@angular/router';

export const POLICY_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/policy-list/policy-list').then(m => m.PolicyListPage) },
  { path: 'renewals', loadComponent: () => import('./pages/renewal-list/renewal-list').then(m => m.RenewalListPage) },
  { path: ':id/endorsement', loadComponent: () => import('./pages/endorsement-form/endorsement-form').then(m => m.EndorsementFormPage) },
  { path: ':id/cancellation', loadComponent: () => import('./pages/cancellation-form/cancellation-form').then(m => m.CancellationFormPage) },
  { path: ':id', loadComponent: () => import('./pages/policy-detail/policy-detail').then(m => m.PolicyDetailPage) },
];
