import { Routes } from '@angular/router';
import { roleGuard } from '../../core/auth/role.guard';

export const SUBMISSION_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/submission-list/submission-list').then(m => m.SubmissionListPage) },
  {
    path: 'new',
    canActivate: [roleGuard], data: { roles: ['Agent'] },
    loadComponent: () => import('./pages/submission-create/submission-create').then(m => m.SubmissionCreatePage),
  },
  { path: ':id',              loadComponent: () => import('./pages/submission-detail/submission-detail').then(m => m.SubmissionDetailPage) },
  { path: ':id/questionnaire', loadComponent: () => import('./pages/questionnaire/questionnaire').then(m => m.QuestionnairePage) },
];
