import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Policy, Renewal, PolicyStatus, RenewalStatus } from '../models/policy.model';
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

  generatePolicyNumber() {
    return this.http.get<{ policyNumber: string }>(`${this.base}/policies/generate-number`);
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
