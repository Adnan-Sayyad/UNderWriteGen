import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { map } from 'rxjs/operators';
import {
  ComplianceChecklist, CreateChecklistPayload, UpdateChecklistPayload, UpdateChecklistStatusPayload,
  ExceptionLog, CreateExceptionPayload, UpdateExceptionStatusPayload,
} from '../models/compliance.model';

// ── Response wrapper: backend may return camelCase OR PascalCase ──────────
function unwrap<T>(r: any): T {
  return (r?.data ?? r?.Data ?? r) as T;
}

function unwrapArray<T>(r: any, normalize: (x: any) => T): T[] {
  // Handle ApiResponseDto<IEnumerable<T>>: { success, message, data: [...] }
  const raw = r?.data ?? r?.Data;

  // data is an array — normal case
  if (Array.isArray(raw)) return raw.map(normalize);

  // data is a single object (shouldn't happen for list endpoints, but guard anyway)
  if (raw && typeof raw === 'object') return [normalize(raw)];

  // The whole response is an array (no wrapper)
  if (Array.isArray(r)) return r.map(normalize);

  // Fallback: log to console so the developer can see what came back
  if (r !== null && r !== undefined) {
    console.warn('[ComplianceApiService] unwrapArray: unexpected response shape', r);
  }
  return [];
}

// ── Normalise a raw checklist object ─────────────────────────────────────
function normalizeChecklist(r: any): ComplianceChecklist {
  return {
    checklistId:   r.checklistId   ?? r.ChecklistId   ?? '',
    submissionId:  r.submissionId  ?? r.SubmissionId  ?? '',
    itemsJson:     r.itemsJson     ?? r.ItemsJson     ?? '[]',
    completedBy:   r.completedBy   ?? r.CompletedBy,
    completedDate: r.completedDate ?? r.CompletedDate,
    status:        r.status        ?? r.Status        ?? 'Pending',
    createdAt:     r.createdAt     ?? r.CreatedAt     ?? '',
    updatedAt:     r.updatedAt     ?? r.UpdatedAt,
  };
}

// ── Normalise a raw exception object ─────────────────────────────────────
function normalizeException(r: any): ExceptionLog {
  return {
    exceptionId:  r.exceptionId  ?? r.ExceptionId  ?? '',
    submissionId: r.submissionId ?? r.SubmissionId ?? '',
    category:     r.category     ?? r.Category     ?? 'Data',
    details:      r.details      ?? r.Details      ?? '',
    loggedDate:   r.loggedDate   ?? r.LoggedDate   ?? '',
    status:       r.status       ?? r.Status       ?? 'Open',
    createdAt:    r.createdAt    ?? r.CreatedAt     ?? '',
    updatedAt:    r.updatedAt    ?? r.UpdatedAt,
  };
}

@Injectable({ providedIn: 'root' })
export class ComplianceApiService {
  private readonly cl = `${environment.apiBaseUrl}/compliance-checklists`;
  private readonly el = `${environment.apiBaseUrl}/exception-logs`;

  constructor(private http: HttpClient) {}

  // ── Compliance Checklists ──────────────────────────────────────────────
  getChecklists() {
    return this.http.get<any>(this.cl).pipe(
      map(r => {
        console.log('[ComplianceAPI] GET checklists raw:', r);
        return unwrapArray<ComplianceChecklist>(r, normalizeChecklist);
      })
    );
  }

  getChecklistById(id: string) {
    return this.http.get<any>(`${this.cl}/${id}`).pipe(
      map(r => normalizeChecklist(unwrap<any>(r)))
    );
  }

  getChecklistBySubmission(submissionId: string) {
    return this.http.get<any>(`${this.cl}/submission/${submissionId}`).pipe(
      map(r => normalizeChecklist(unwrap<any>(r)))
    );
  }

  createChecklist(payload: CreateChecklistPayload) {
    // Send PascalCase keys — works with both old (no case-insensitive) and new backend
    const body = {
      SubmissionId:  payload.submissionId,
      ItemsJson:     payload.itemsJson,
      CompletedBy:   payload.completedBy,
      CompletedDate: payload.completedDate,
      Status:        payload.status,
    };
    return this.http.post<any>(this.cl, body).pipe(
      map(r => normalizeChecklist(unwrap<any>(r)))
    );
  }

  updateChecklist(id: string, payload: UpdateChecklistPayload) {
    const body = {
      ItemsJson:     payload.itemsJson,
      CompletedBy:   payload.completedBy,
      CompletedDate: payload.completedDate,
    };
    return this.http.put<any>(`${this.cl}/${id}`, body).pipe(
      map(r => normalizeChecklist(unwrap<any>(r)))
    );
  }

  updateChecklistStatus(id: string, payload: UpdateChecklistStatusPayload) {
    return this.http.patch<any>(`${this.cl}/${id}/status`, { Status: payload.status }).pipe(
      map(r => normalizeChecklist(unwrap<any>(r)))
    );
  }

  // ── Exception Logs ────────────────────────────────────────────────────
  getExceptions() {
    return this.http.get<any>(this.el).pipe(
      map(r => {
        console.log('[ComplianceAPI] GET exceptions raw:', r);
        return unwrapArray<ExceptionLog>(r, normalizeException);
      })
    );
  }

  getExceptionsByCategory(category: string) {
    return this.http.get<any>(`${this.el}/category/${category}`).pipe(
      map(r => unwrapArray<ExceptionLog>(r, normalizeException))
    );
  }

  createException(payload: CreateExceptionPayload) {
    const body = {
      SubmissionId: payload.submissionId,
      Category:     payload.category,
      Details:      payload.details,
    };
    return this.http.post<any>(this.el, body).pipe(
      map(r => normalizeException(unwrap<any>(r)))
    );
  }

  updateExceptionStatus(id: string, payload: UpdateExceptionStatusPayload) {
    return this.http.patch<any>(`${this.el}/${id}/status`, { Status: payload.status }).pipe(
      map(r => normalizeException(unwrap<any>(r)))
    );
  }
}
