import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Policy, Endorsement, Cancellation, Renewal, PolicyStatus, EndorsementStatus, CancellationStatus, RenewalStatus, EndorsementType } from '../models/policy.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';
import { toPagedResponse } from '../../../shared/models/paging.util';

function parseJSON(val: any): Record<string, unknown> {
  if (!val) return {};
  if (typeof val === 'string') { try { return JSON.parse(val); } catch { return {}; } }
  return val;
}

function normalizePolicy(r: any): Policy {
  return {
    policyId:      r.policyId      ?? r.policyID      ?? r.PolicyID      ?? '',
    submissionId:  r.submissionId  ?? r.submissionID  ?? r.SubmissionID  ?? '',
    policyNumber:  r.policyNumber  ?? r.PolicyNumber  ?? '',
    productLine:   r.productLine   ?? r.ProductLine   ?? '',
    coverageJSON:  parseJSON(r.coverageJSON ?? r.CoverageJSON),
    inceptionDate: r.inceptionDate ?? r.InceptionDate ?? '',
    expiryDate:    r.expiryDate    ?? r.ExpiryDate    ?? '',
    status:        (r.status       ?? r.Status        ?? 'Active') as PolicyStatus,
  };
}

function normalizeEndorsement(r: any): Endorsement {
  return {
    endorsementId:   r.endorsementId   ?? r.endorsementID   ?? r.EndorsementID   ?? '',
    policyId:        r.policyId        ?? r.policyID        ?? r.PolicyID        ?? '',
    endorsementType: (r.endorsementType ?? r.EndorsementType ?? 'MidTermChange') as EndorsementType,
    changesJSON:     parseJSON(r.changesJSON ?? r.ChangesJSON),
    effectiveDate:   r.effectiveDate   ?? r.EffectiveDate   ?? '',
    premiumDelta:    r.premiumDelta     ?? r.PremiumDelta    ?? 0,
    status:          (r.status         ?? r.Status          ?? 'Proposed') as EndorsementStatus,
  };
}

function normalizeCancellation(r: any): Cancellation {
  return {
    cancellationId: r.cancellationId ?? r.cancellationID ?? r.CancellationID ?? '',
    policyId:       r.policyId       ?? r.policyID       ?? r.PolicyID       ?? '',
    cancelReason:   r.cancelReason   ?? r.CancelReason   ?? '',
    cancelDate:     r.cancelDate     ?? r.CancelDate     ?? '',
    refundPremium:  r.refundPremium  ?? r.RefundPremium  ?? 0,
    status:         (r.status        ?? r.Status         ?? 'Requested') as CancellationStatus,
  };
}

function normalizeRenewal(r: any): Renewal {
  return {
    renewalId:        r.renewalId        ?? r.renewalID        ?? r.RenewalID        ?? '',
    policyId:         r.policyId         ?? r.policyID         ?? r.PolicyID         ?? '',
    renewalOfferJSON: parseJSON(r.renewalOfferJSON ?? r.RenewalOfferJSON),
    offeredDate:      r.offeredDate      ?? r.OfferedDate      ?? '',
    status:           (r.status          ?? r.Status           ?? 'Offered') as RenewalStatus,
  };
}

const DEFAULT_REQ: PageRequest = { page: 0, size: 10 };

@Injectable({ providedIn: 'root' })
export class PolicyApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  // ── Policies ──────────────────────────────────────────────────────────────

  getAll(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<any>(`${this.base}/policies`, {
      params: new HttpParams({ fromObject: { ...req, ...filters } as any }),
    }).pipe(map(res => toPagedResponse<Policy>(res, req, normalizePolicy)));
  }

  getById(id: string) {
    return this.http.get<any>(`${this.base}/policies/${id}`).pipe(
      map(res => ({ data: normalizePolicy(res?.data ?? res) } as ApiResponse<Policy>))
    );
  }

  bind(submissionId: string, policyNumber: string, productLine: string, inceptionDate: string, expiryDate: string) {
    return this.http.post<any>(`${this.base}/policies`, {
      submissionID:  submissionId,
      policyNumber,
      productLine,
      coverageJSON:  '{}',
      inceptionDate,
      expiryDate,
    }).pipe(map(res => ({ data: normalizePolicy(res?.data ?? res) } as ApiResponse<Policy>)));
  }

  // ── Endorsements ──────────────────────────────────────────────────────────

  getEndorsements(policyId: string) {
    return this.http.get<any>(`${this.base}/endorsements/${policyId}`).pipe(
      map(res => toPagedResponse<Endorsement>(res, DEFAULT_REQ, normalizeEndorsement))
    );
  }

  createEndorsement(policyId: string, payload: Partial<Endorsement>) {
    return this.http.post<any>(`${this.base}/endorsements`, {
      policyID:        policyId,
      endorsementType: payload.endorsementType,
      changesJSON:     typeof payload.changesJSON === 'object' ? JSON.stringify(payload.changesJSON) : (payload.changesJSON ?? '{}'),
      effectiveDate:   payload.effectiveDate,
      premiumDelta:    payload.premiumDelta ?? 0,
    }).pipe(map(res => ({ data: normalizeEndorsement(res?.data ?? res) } as ApiResponse<Endorsement>)));
  }

  approveEndorsement(id: string) {
    return this.http.patch<any>(`${this.base}/endorsements/${id}/status`, { status: 'Approved' }).pipe(
      map(res => ({ data: normalizeEndorsement(res?.data ?? res) } as ApiResponse<Endorsement>))
    );
  }

  // ── Cancellations ─────────────────────────────────────────────────────────

  getCancellations(policyId: string) {
    return this.http.get<any>(`${this.base}/cancellations/${policyId}`).pipe(
      map(res => {
        const item = res?.data ?? res;
        const raw = Array.isArray(item) ? item : (item ? [item] : []);
        return toPagedResponse<Cancellation>(raw, DEFAULT_REQ, normalizeCancellation);
      })
    );
  }

  requestCancellation(policyId: string, payload: Partial<Cancellation>) {
    return this.http.post<any>(`${this.base}/cancellations`, {
      policyID:      policyId,
      cancelReason:  payload.cancelReason,
      cancelDate:    payload.cancelDate,
      refundPremium: payload.refundPremium ?? 0,
    }).pipe(map(res => ({ data: normalizeCancellation(res?.data ?? res) } as ApiResponse<Cancellation>)));
  }

  approveCancellation(id: string) {
    return this.http.patch<any>(`${this.base}/cancellations/${id}/status`, { status: 'Approved' }).pipe(
      map(res => ({ data: normalizeCancellation(res?.data ?? res) } as ApiResponse<Cancellation>))
    );
  }

  // ── Renewals ──────────────────────────────────────────────────────────────

  getRenewals(req: PageRequest) {
    return this.http.get<any>(`${this.base}/renewals`, {
      params: new HttpParams({ fromObject: { ...req } as any }),
    }).pipe(map(res => toPagedResponse<Renewal>(res, req, normalizeRenewal)));
  }

  updateRenewal(id: string, payload: Partial<Renewal>) {
    return this.http.put<any>(`${this.base}/renewals/${id}`, payload).pipe(
      map(res => ({ data: normalizeRenewal(res?.data ?? res) } as ApiResponse<Renewal>))
    );
  }
}
