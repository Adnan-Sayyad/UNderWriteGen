import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const COMPLIANCE_ROUTES: Routes = [
  { path: '', redirectTo: 'checklists', pathMatch: 'full' },

  // Checklists: Compliance + UWAssistant + Admin
  { path: 'checklists', loadComponent: () => import('./pages/checklist/checklist').then(m => m.ChecklistPage) },

  // Authority Breaches: Compliance + Admin only
  {
    path: 'authority-breaches',
    canActivate: [roleGuard], data: { roles: ['Compliance', 'Admin'] },
    loadComponent: () => import('./pages/authority-breaches/authority-breaches').then(m => m.AuthorityBreachesPage),
  },

  // Exception Log: Compliance + Admin only
  {
    path: 'exceptions',
    canActivate: [roleGuard], data: { roles: ['Compliance', 'Admin'] },
    loadComponent: () => import('./pages/exception-log/exception-log').then(m => m.ExceptionLogPage),
  },
];
