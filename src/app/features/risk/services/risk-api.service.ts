import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { RiskProfile, EvidenceRef, RiskType, EvidenceStatus } from '../models/risk.model';
import { ApiResponse } from '../../../shared/models/api-response.model';

function normalizeRiskProfile(r: any): RiskProfile {
  let attrs = r.attributesJSON ?? r.AttributesJSON ?? {};
  if (typeof attrs === 'string') { try { attrs = JSON.parse(attrs); } catch { attrs = {}; } }
  return {
    riskId:        r.riskId       ?? r.riskID       ?? r.RiskID       ?? '',
    submissionId:  r.submissionId ?? r.submissionID ?? r.SubmissionID ?? '',
    riskType:      (r.riskType    ?? r.RiskType     ?? 'Life') as RiskType,
    attributesJSON: attrs,
    riskNotes:     r.riskNotes    ?? r.RiskNotes    ?? '',
    lastUpdated:   r.lastUpdated  ?? r.LastUpdated  ?? new Date().toISOString(),
  };
}

function normalizeEvidence(r: any): EvidenceRef {
  let result = r.resultJSON ?? r.ResultJSON ?? {};
  if (typeof result === 'string') { try { result = JSON.parse(result); } catch { result = {}; } }
  return {
    evidenceId:   r.evidenceId   ?? r.evidenceID   ?? r.EvidenceID   ?? '',
    submissionId: r.submissionId ?? r.submissionID ?? r.SubmissionID ?? '',
    evidenceType: r.evidenceType ?? r.EvidenceType ?? 'InspectionReport',
    provider:     r.provider     ?? r.Provider     ?? '',
    referenceNo:  r.referenceNo  ?? r.ReferenceNo  ?? '',
    resultJSON:   result,
    receivedDate: r.receivedDate ?? r.ReceivedDate ?? '',
    status:       (r.status      ?? r.Status       ?? 'Requested') as EvidenceStatus,
  };
}

@Injectable({ providedIn: 'root' })
export class RiskApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  // ── Risk Profiles ──────────────────────────────────────────────────────────

  getRiskProfile(submissionId: string) {
    return this.http.get<any>(`${this.base}/risk-profiles/submission/${submissionId}`).pipe(
      map(res => {
        const raw = res?.data ?? res;
        return { data: raw ? normalizeRiskProfile(raw) : null } as ApiResponse<RiskProfile>;
      })
    );
  }

  createRiskProfile(submissionId: string, payload: Partial<RiskProfile>) {
    return this.http.post<any>(`${this.base}/risk-profiles`, {
      submissionID:  submissionId,
      riskType:      payload.riskType ?? 'Life',
      attributesJSON: JSON.stringify(payload.attributesJSON ?? {}),
      riskNotes:     payload.riskNotes ?? '',
    }).pipe(map(res => ({ data: normalizeRiskProfile(res?.data ?? res) } as ApiResponse<RiskProfile>)));
  }

  updateRiskProfile(riskId: string, payload: Partial<RiskProfile>) {
    return this.http.put<any>(`${this.base}/risk-profiles/${riskId}`, {
      riskType:      payload.riskType ?? 'Life',
      attributesJSON: JSON.stringify(payload.attributesJSON ?? {}),
      riskNotes:     payload.riskNotes ?? '',
    }).pipe(map(res => ({ data: normalizeRiskProfile(res?.data ?? res) } as ApiResponse<RiskProfile>)));
  }

  // ── Evidence Refs ──────────────────────────────────────────────────────────

  getEvidence(submissionId: string) {
    return this.http.get<any>(`${this.base}/evidence-refs/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeEvidence) } as ApiResponse<EvidenceRef[]>;
      })
    );
  }

  addEvidence(submissionId: string, payload: Partial<EvidenceRef>) {
    return this.http.post<any>(`${this.base}/evidence-refs`, {
      submissionID:  submissionId,
      evidenceType:  payload.evidenceType,
      provider:      payload.provider,
      referenceNo:   payload.referenceNo,
      resultJSON:    JSON.stringify(payload.resultJSON ?? {}),
      status:        payload.status ?? 'Requested',
    }).pipe(map(res => ({ data: normalizeEvidence(res?.data ?? res) } as ApiResponse<EvidenceRef>)));
  }

  updateEvidence(evidenceId: string, payload: Partial<EvidenceRef>) {
    return this.http.patch<any>(`${this.base}/evidence-refs/${evidenceId}/status`, {
      status:     payload.status,
      resultJSON: payload.resultJSON ? JSON.stringify(payload.resultJSON) : undefined,
    }).pipe(map(res => ({ data: normalizeEvidence(res?.data ?? res) } as ApiResponse<EvidenceRef>)));
  }

  // No DELETE endpoint in backend — mark as NotAvailable instead
  deleteEvidence(evidenceId: string) {
    return this.http.patch<any>(`${this.base}/evidence-refs/${evidenceId}/status`, {
      status: 'NotAvailable',
    }).pipe(map(() => ({ success: true, data: null } as unknown as ApiResponse<void>)));
  }
}
