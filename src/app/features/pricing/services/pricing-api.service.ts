import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import {
  PricingParam,
  Quote,
  CreateQuoteRequest,
  UpdateQuoteTermsRequest,
  UpdateQuoteStatusRequest,
  CreatePricingParamRequest,
  UpdatePricingParamRequest,
  QuoteActionResponse,
} from '../models/pricing.model';

// All API calls are made under the /api prefix to avoid colliding with Angular
// route paths (e.g. /submissions, /notifications, /reports). The dev proxy
// (proxy.conf.json) forwards /api/* to the appropriate backend microservice.

@Injectable({ providedIn: 'root' })
export class PricingApiService {
  private readonly base = environment.apiBaseUrl; // '' in dev (proxy handles /api rewrite)

  constructor(private http: HttpClient) {}

  // ── Quotes ──────────────────────────────────────────────────────────────────

  /** POST /api/quotes — triggers the backend pricing engine */
  generateQuote(payload: CreateQuoteRequest) {
    return this.http.post<Quote>(`${this.base}/quotes`, payload);
  }

  /** GET /api/quotes/{quoteId} */
  getQuote(id: string) {
    return this.http.get<Quote>(`${this.base}/quotes/${id}`);
  }

  /** GET /api/quotes/submission/{submissionId}/latest */
  getLatestQuoteForSubmission(submissionId: string) {
    return this.http.get<Quote>(`${this.base}/quotes/submission/${submissionId}/latest`);
  }

  /** POST /api/quotes/{quoteId}/accept */
  acceptQuote(quoteId: string) {
    return this.http.post<QuoteActionResponse>(`${this.base}/quotes/${quoteId}/accept`, {});
  }

  /** PATCH /api/quotes/{quoteId}/terms */
  updateTerms(quoteId: string, payload: UpdateQuoteTermsRequest) {
    return this.http.patch<QuoteActionResponse>(`${this.base}/quotes/${quoteId}/terms`, payload);
  }

  /** PATCH /api/quotes/{quoteId}/status */
  updateStatus(quoteId: string, payload: UpdateQuoteStatusRequest) {
    return this.http.patch<QuoteActionResponse>(`${this.base}/quotes/${quoteId}/status`, payload);
  }

  // ── Pricing Parameters ──────────────────────────────────────────────────────

  /** GET /api/pricing-params */
  getParams() {
    return this.http.get<PricingParam[]>(`${this.base}/pricing-params`);
  }

  /** GET /api/pricing-params/product-line/{line} */
  getParamsByProductLine(line: string) {
    return this.http.get<PricingParam[]>(`${this.base}/pricing-params/product-line/${line}`);
  }

  /** GET /api/pricing-params/effective/{date} (format: yyyy-MM-dd) */
  getParamsByEffectiveDate(date: string) {
    return this.http.get<PricingParam[]>(`${this.base}/pricing-params/effective/${date}`);
  }

  /** POST /api/pricing-params */
  createParam(payload: CreatePricingParamRequest) {
    return this.http.post<PricingParam>(`${this.base}/pricing-params`, payload);
  }

  /** PUT /api/pricing-params/{paramId} — only value + description are updated */
  updateParam(id: string, payload: UpdatePricingParamRequest) {
    return this.http.put<PricingParam>(`${this.base}/pricing-params/${id}`, payload);
  }

  /** DELETE /api/pricing-params/{paramId} */
  deleteParam(id: string) {
    return this.http.delete<void>(`${this.base}/pricing-params/${id}`);
  }

  /** GET /api/quotes/submission/{submissionId} — all versions, newest first */
  getQuotesBySubmission(submissionId: string) {
    return this.http.get<Quote[]>(`${this.base}/quotes/submission/${submissionId}`);
  }

  /** GET /api/quotes?status=Accepted — all accepted quotes for policy binding */
  getAllQuotes(status?: string) {
    const url = status
      ? `${this.base}/quotes?status=${status}`
      : `${this.base}/quotes`;
    return this.http.get<any>(url);
  }
}
