import { Routes } from '@angular/router';

export const COMPLIANCE_ROUTES: Routes = [
  { path: '', redirectTo: 'checklists', pathMatch: 'full' },
  { path: 'checklists',         loadComponent: () => import('./pages/checklist/checklist').then(m => m.ChecklistPage) },
  { path: 'authority-breaches', loadComponent: () => import('./pages/authority-breaches/authority-breaches').then(m => m.AuthorityBreachesPage) },
  { path: 'exceptions',         loadComponent: () => import('./pages/exception-log/exception-log').then(m => m.ExceptionLogPage) },
  { path: 'audit-log',          loadComponent: () => import('./pages/audit-log/audit-log').then(m => m.AuditLogPage) },
];
