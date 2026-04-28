import { Routes } from '@angular/router';

export const PARTY_ROUTES: Routes = [
  { path: '', redirectTo: 'agents', pathMatch: 'full' },
  {
    path: 'agents',
    loadComponent: () => import('./pages/agent-list/agent-list').then(m => m.AgentListPage),
  },
  {
    path: 'agents/new',
    loadComponent: () => import('./pages/agent-form/agent-form').then(m => m.AgentFormPage),
  },
  {
    path: 'agents/:id',
    loadComponent: () => import('./pages/agent-form/agent-form').then(m => m.AgentFormPage),
  },
  {
    path: 'parties',
    loadComponent: () => import('./pages/party-list/party-list').then(m => m.PartyListPage),
  },
  {
    path: 'parties/new',
    loadComponent: () => import('./pages/party-form/party-form').then(m => m.PartyFormPage),
  },
  {
    path: 'parties/:id',
    loadComponent: () => import('./pages/party-form/party-form').then(m => m.PartyFormPage),
  },
];
