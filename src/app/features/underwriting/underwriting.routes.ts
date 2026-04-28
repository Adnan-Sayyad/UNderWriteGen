import { Routes } from '@angular/router';

export const UNDERWRITING_ROUTES: Routes = [
  { path: '', redirectTo: 'workbench', pathMatch: 'full' },
  { path: 'workbench',      loadComponent: () => import('./pages/uw-workbench/uw-workbench').then(m => m.UwWorkbenchPage) },
  { path: 'decision/:submissionId', loadComponent: () => import('./pages/uw-decision/uw-decision').then(m => m.UwDecisionPage) },
  { path: 'subjectivities', loadComponent: () => import('./pages/subjectivity-list/subjectivity-list').then(m => m.SubjectivityListPage) },
];
