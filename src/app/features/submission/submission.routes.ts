import { Routes } from '@angular/router';

export const SUBMISSION_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/submission-list/submission-list').then(m => m.SubmissionListPage) },
  { path: 'new', loadComponent: () => import('./pages/submission-create/submission-create').then(m => m.SubmissionCreatePage) },
  { path: ':id', loadComponent: () => import('./pages/submission-detail/submission-detail').then(m => m.SubmissionDetailPage) },
  { path: ':id/questionnaire', loadComponent: () => import('./pages/questionnaire/questionnaire').then(m => m.QuestionnairePage) },
];
