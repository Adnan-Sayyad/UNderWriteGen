import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  // Public landing page (default)
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/landing/landing').then(m => m.LandingPage),
  },

  // Auth shell (public)
  {
    path: 'auth',
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then(m => m.AuthLayout),
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },

  // App shell (authenticated)
  {
    path: '',
    loadComponent: () => import('./layouts/main-layout/main-layout').then(m => m.MainLayout),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'submissions', pathMatch: 'full' },

      { path: 'party',         loadChildren: () => import('./features/party/party.routes').then(m => m.PARTY_ROUTES) },
      { path: 'submissions',   loadChildren: () => import('./features/submission/submission.routes').then(m => m.SUBMISSION_ROUTES) },
      { path: 'risk',          loadChildren: () => import('./features/risk/risk.routes').then(m => m.RISK_ROUTES) },
      {
        path: 'rules',
        canActivate: [roleGuard], data: { roles: ['Underwriter', 'Admin'] },
        loadChildren: () => import('./features/rules/rules.routes').then(m => m.RULES_ROUTES),
      },
      {
        path: 'pricing',
        canActivate: [roleGuard], data: { roles: ['PricingAnalyst', 'Underwriter', 'Admin'] },
        loadChildren: () => import('./features/pricing/pricing.routes').then(m => m.PRICING_ROUTES),
      },
      {
        path: 'underwriting',
 
        canActivate: [roleGuard], data: { roles: ['Underwriter', 'UWAssistant', 'Admin'] },
 
        loadChildren: () => import('./features/underwriting/underwriting.routes').then(m => m.UNDERWRITING_ROUTES),
      },
      {
        path: 'policy',
        canActivate: [roleGuard], data: { roles: ['Operations', 'Underwriter', 'Admin'] },
        loadChildren: () => import('./features/policy/policy.routes').then(m => m.POLICY_ROUTES),
      },
      {
        path: 'compliance',
        canActivate: [roleGuard], data: { roles: ['Compliance', 'Admin'] },
        loadChildren: () => import('./features/compliance/compliance.routes').then(m => m.COMPLIANCE_ROUTES),
      },
      { path: 'reports',       loadChildren: () => import('./features/reports/reports.routes').then(m => m.REPORTS_ROUTES) },
      { path: 'notifications', loadChildren: () => import('./features/notifications/notifications.routes').then(m => m.NOTIFICATIONS_ROUTES) },
      { path: 'profile',       loadComponent: () => import('./features/auth/pages/profile/profile').then(m => m.ProfilePage) },
      { path: 'access-denied', loadComponent: () => import('./features/access-denied/access-denied').then(m => m.AccessDeniedPage) },
      {
        path: 'admin',
        canActivate: [roleGuard], data: { roles: ['Admin'] },
        loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
      },
    ],
  },

  { path: '**', redirectTo: '' },
];
