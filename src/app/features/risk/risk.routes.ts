import { Routes } from '@angular/router';

export const RISK_ROUTES: Routes = [
  { path: '', redirectTo: 'profiles', pathMatch: 'full' },
  { path: 'profiles/:submissionId', loadComponent: () => import('./pages/risk-profile/risk-profile').then(m => m.RiskProfilePage) },
  { path: 'evidence/:submissionId', loadComponent: () => import('./pages/evidence-ref/evidence-ref').then(m => m.EvidenceRefPage) },
];
