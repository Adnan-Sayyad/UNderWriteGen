import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'users', pathMatch: 'full' },
  { path: 'users',      loadComponent: () => import('./pages/user-management/user-management').then(m => m.UserManagementPage) },
  { path: 'audit-logs', loadComponent: () => import('./pages/audit-logs/audit-logs').then(m => m.AuditLogsPage) },
  { path: 'products',   loadComponent: () => import('./pages/product-config/product-config').then(m => m.ProductConfigPage) },
  { path: 'slas',       loadComponent: () => import('./pages/sla-config/sla-config').then(m => m.SlaConfigPage) },
  { path: 'templates',  loadComponent: () => import('./pages/template-config/template-config').then(m => m.TemplateConfigPage) },
];
