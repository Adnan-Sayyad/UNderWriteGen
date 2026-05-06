import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { map } from 'rxjs/operators';
import {
  ComplianceChecklist, CreateChecklistPayload, UpdateChecklistPayload, UpdateChecklistStatusPayload,
  AuthorityBreach, CreateBreachPayload, UpdateBreachStatusPayload,
  ExceptionLog, CreateExceptionPayload, UpdateExceptionStatusPayload,
} from '../models/compliance.model';
import { AuditLogDto } from '../../../core/services/iam-api.service';

interface ApiResp<T> { success: boolean; message: string; data: T; }

@Injectable({ providedIn: 'root' })
export class ComplianceApiService {
  private readonly cl  = `${environment.apiBaseUrl}/compliance-checklists`;
  private readonly ab  = `${environment.apiBaseUrl}/authority-breaches`;
  private readonly el  = `${environment.apiBaseUrl}/exception-logs`;
  private readonly aud = `${environment.apiBaseUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  // ── Compliance Checklists ──────────────────────────────────────
  getChecklists() {
    return this.http.get<ApiResp<ComplianceChecklist[]>>(this.cl).pipe(map(r => r.data ?? []));
  }
  getChecklistById(id: string) {
    return this.http.get<ApiResp<ComplianceChecklist>>(`${this.cl}/${id}`).pipe(map(r => r.data));
  }
  getChecklistBySubmission(submissionId: string) {
    return this.http.get<ApiResp<ComplianceChecklist>>(`${this.cl}/submission/${submissionId}`).pipe(map(r => r.data));
  }
  createChecklist(payload: CreateChecklistPayload) {
    return this.http.post<ApiResp<ComplianceChecklist>>(this.cl, payload).pipe(map(r => r.data));
  }
  updateChecklist(id: string, payload: UpdateChecklistPayload) {
    return this.http.put<ApiResp<ComplianceChecklist>>(`${this.cl}/${id}`, payload).pipe(map(r => r.data));
  }
  updateChecklistStatus(id: string, payload: UpdateChecklistStatusPayload) {
    return this.http.patch<ApiResp<ComplianceChecklist>>(`${this.cl}/${id}/status`, payload).pipe(map(r => r.data));
  }

  // ── Authority Breaches ────────────────────────────────────────
  getBreaches() {
    return this.http.get<ApiResp<AuthorityBreach[]>>(this.ab).pipe(map(r => r.data ?? []));
  }
  getBreachesByType(breachType: string) {
    return this.http.get<ApiResp<AuthorityBreach[]>>(`${this.ab}/type/${breachType}`).pipe(map(r => r.data ?? []));
  }
  createBreach(payload: CreateBreachPayload) {
    return this.http.post<ApiResp<AuthorityBreach>>(this.ab, payload).pipe(map(r => r.data));
  }
  updateBreachStatus(id: string, payload: UpdateBreachStatusPayload) {
    return this.http.patch<ApiResp<AuthorityBreach>>(`${this.ab}/${id}/status`, payload).pipe(map(r => r.data));
  }

  // ── Exception Logs ────────────────────────────────────────────
  getExceptions() {
    return this.http.get<ApiResp<ExceptionLog[]>>(this.el).pipe(map(r => r.data ?? []));
  }
  getExceptionsByCategory(category: string) {
    return this.http.get<ApiResp<ExceptionLog[]>>(`${this.el}/category/${category}`).pipe(map(r => r.data ?? []));
  }
  createException(payload: CreateExceptionPayload) {
    return this.http.post<ApiResp<ExceptionLog>>(this.el, payload).pipe(map(r => r.data));
  }
  updateExceptionStatus(id: string, payload: UpdateExceptionStatusPayload) {
    return this.http.patch<ApiResp<ExceptionLog>>(`${this.el}/${id}/status`, payload).pipe(map(r => r.data));
  }

  // ── Audit Logs (from IAM service) ─────────────────────────────
  getAuditLogs(adminId: string) {
    return this.http.get<ApiResp<AuditLogDto[]>>(this.aud, {
      params: { adminId },
    }).pipe(map(r => r.data ?? []));
  }
}
