import { Routes } from '@angular/router';

export const PRICING_ROUTES: Routes = [
  { path: '', redirectTo: 'quotes', pathMatch: 'full' },
  { path: 'params',       loadComponent: () => import('./pages/pricing-params/pricing-params').then(m => m.PricingParamsPage) },
  { path: 'quotes',       loadComponent: () => import('./pages/quote-list/quote-list').then(m => m.QuoteListPage) },
  { path: 'quotes/new/:submissionId', loadComponent: () => import('./pages/quote-create/quote-create').then(m => m.QuoteCreatePage) },
  { path: 'quotes/:id',   loadComponent: () => import('./pages/quote-detail/quote-detail').then(m => m.QuoteDetailPage) },
];
