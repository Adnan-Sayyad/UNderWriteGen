// ── Quote ─────────────────────────────────────────────────────────────────────
// Status values match backend QuoteStatus enum exactly
export type QuoteStatus = 'Draft' | 'Presented' | 'Accepted' | 'Declined' | 'Expired';

// Matches QuoteResponse DTO from backend (GET /api/quotes/{id})
export interface Quote {
  quoteRef?: string;   // e.g. QUO-2026-04287 — display reference only, use quoteId for API calls
  quoteId: string;
  submissionId: string;
  versionNo: number;
  basePremium: number;
  totalPremium: number;
  validUntil: string;
  status: QuoteStatus;
  // JSON strings from backend — parse with JSON.parse() before use
  loadingsJson: string;
  discountsJson: string;
  taxesJson: string;
  // Present only when terms have been attached via PATCH /terms
  termsJson?: string;
}

// Parsed structure of termsJson
export interface QuoteTerms {
  deductibles?: number;
  limits?: Record<string, number>;
  exclusions?: string[];
  subjectivities?: string[];
}

// ── Requests ──────────────────────────────────────────────────────────────────

// POST /api/quotes — backend pricing engine generates all premiums automatically
export interface CreateQuoteRequest {
  submissionId: string;
  applyDiscounts: boolean;
  applyTaxes: boolean;
  requestedBy: string;
}

// PATCH /api/quotes/{quoteId}/terms
export interface UpdateQuoteTermsRequest {
  quoteId: string;
  termsJson: string;
}

// PATCH /api/quotes/{quoteId}/status
export interface UpdateQuoteStatusRequest {
  status: string;
  reason?: string;
}

// ── Pricing Parameter ─────────────────────────────────────────────────────────
// Matches PricingParamResponse DTO from backend
export interface PricingParam {
  id: string;
  productLine: string;
  paramName: string;
  value: number;
  description: string;
  effectiveFrom: string;
  effectiveTo?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

// POST /api/pricing-params
export interface CreatePricingParamRequest {
  productLine: string;
  paramName: string;
  value: number;
  description: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
}

// PUT /api/pricing-params/{id} — only value and description can be updated
export interface UpdatePricingParamRequest {
  value: number;
  description: string;
}

// ── Accept/Update response ────────────────────────────────────────────────────
export interface QuoteActionResponse {
  message: string;
  quoteId: string;
}
