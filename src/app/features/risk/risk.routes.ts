import { Routes } from '@angular/router';

export const RISK_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/risk-landing/risk-landing').then(m => m.RiskLandingPage) },
  { path: 'profiles/:submissionId', loadComponent: () => import('./pages/risk-profile/risk-profile').then(m => m.RiskProfilePage) },
  { path: 'evidence/:submissionId', loadComponent: () => import('./pages/evidence-ref/evidence-ref').then(m => m.EvidenceRefPage) },
];
