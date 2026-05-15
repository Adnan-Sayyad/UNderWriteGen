import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const RULES_ROUTES: Routes = [
  { path: '', redirectTo: 'list', pathMatch: 'full' },
  { path: 'list',            loadComponent: () => import('./pages/rule-list/rule-list').then(m => m.RuleListPage) },
  {
    path: 'new',
    canActivate: [roleGuard], data: { roles: ['Operations'] },
    loadComponent: () => import('./pages/rule-form/rule-form').then(m => m.RuleFormPage),
  },
  {
    path: ':id/edit',
    canActivate: [roleGuard], data: { roles: ['Operations'] },
    loadComponent: () => import('./pages/rule-form/rule-form').then(m => m.RuleFormPage),
  },
  { path: 'risk-scores',     loadComponent: () => import('./pages/risk-score/risk-score').then(m => m.RiskScorePage) },
  { path: 'referral-matrix', loadComponent: () => import('./pages/referral-matrix/referral-matrix').then(m => m.ReferralMatrixPage) },
  { path: 'referrals',       loadComponent: () => import('./pages/referral-list/referral-list').then(m => m.ReferralListPage) },
];
