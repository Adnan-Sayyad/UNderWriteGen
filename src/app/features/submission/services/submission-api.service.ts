import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Submission, Attachment, CompletenessCheck, Questionnaire, ProductLine, SubmissionStatus } from '../models/submission.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';
import { AuthService } from '../../../core/auth/auth.service';

const PAGE_SIZE = 10;
const PRODUCTS: ProductLine[]      = ['Life', 'Health', 'PnC', 'Commercial'];
const STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];

function normalizeSubmission(r: any): Submission {
  return {
    submissionId:  r.submissionId  ?? r.submissionID,
    partyId:       r.partyId       ?? r.partyID,
    agentId:       r.agentId       ?? r.agentID,
    productLine:   typeof r.productLine === 'number' ? PRODUCTS[r.productLine]  : r.productLine,
    status:        typeof r.status      === 'number' ? STATUSES[r.status]       : r.status,
    coverageJSON:  typeof r.coverageJSON === 'string' ? JSON.parse(r.coverageJSON) : (r.coverageJSON ?? {}),
    inceptionDate: r.inceptionDate,
    createdDate:   r.createdDate,
  };
}

function normalizeAttachment(r: any): Attachment {
  return {
    attachmentId: r.attachmentId ?? r.attachmentID ?? r.AttachmentID,
    submissionId: r.submissionId ?? r.submissionID,
    docType:      r.docType      ?? r.DocType,
    fileUri:      r.fileUri      ?? r.fileURI      ?? r.FileURI ?? '',
    uploadedBy:   r.uploadedBy   ?? r.UploadedBy   ?? '',
    uploadedDate: r.uploadedDate ?? r.UploadedDate ?? new Date().toISOString(),
  };
}

function normalizeCompleteness(r: any): CompletenessCheck | null {
  if (!r) return null;
  let missing = r.missingItemsJSON ?? r.MissingItemsJSON ?? [];
  if (typeof missing === 'string') {
    try { missing = JSON.parse(missing); } catch { missing = []; }
  }
  return {
    checkId:          r.checkId      ?? r.checkID     ?? r.CheckID ?? '',
    submissionId:     r.submissionId ?? r.submissionID ?? '',
    missingItemsJSON: Array.isArray(missing) ? missing : [],
    status:           r.status       ?? r.Status       ?? 'Pending',
    checkedDate:      r.checkedDate  ?? r.CheckedDate  ?? new Date().toISOString(),
  };
}

function normalizeQuestionnaire(r: any): Questionnaire | null {
  if (!r) return null;
  let responses = r.responsesJSON ?? r.ResponsesJSON ?? {};
  if (typeof responses === 'string') {
    try { responses = JSON.parse(responses); } catch { responses = {}; }
  }
  return {
    qId:             r.qId             ?? r.qID             ?? r.QID             ?? '',
    submissionId:    r.submissionId    ?? r.submissionID    ?? r.SubmissionID    ?? '',
    templateVersion: r.templateVersion ?? r.TemplateVersion ?? '1.0',
    responsesJSON:   responses,
    completedDate:   r.completedDate   ?? r.CompletedDate   ?? new Date().toISOString(),
  };
}

@Injectable({ providedIn: 'root' })
export class SubmissionApiService {
  private readonly base = `${environment.apiBaseUrl}`;
  private readonly auth = inject(AuthService);

  constructor(private http: HttpClient) {}

  // ── Submissions ────────────────────────────────────────────────────────────

  getAll(req: PageRequest, filters?: Record<string, string>) {
    const params = new HttpParams({ fromObject: { ...req, ...filters } as any });
    return this.http.get<any>(`${this.base}/submissions`, { params }).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.content ?? res?.data ?? []);
        const normalized = items.map(normalizeSubmission);
        return {
          content: normalized, data: normalized,
          totalPages:    Array.isArray(res) ? 1 : (res?.totalPages    ?? 1),
          totalElements: Array.isArray(res) ? items.length : (res?.totalElements ?? items.length),
          size: PAGE_SIZE, number: 0,
        } as unknown as PagedResponse<Submission>;
      })
    );
  }

  getById(id: string) {
    return this.http.get<any>(`${this.base}/submissions/${id}`).pipe(
      map(res => ({ data: normalizeSubmission(res?.data ?? res) } as ApiResponse<Submission>))
    );
  }

  create(payload: Partial<Submission>) {
    return this.http.post<ApiResponse<Submission>>(`${this.base}/submissions`, {
      partyID:      payload.partyId,
      agentID:      payload.agentId,
      productLine:  payload.productLine,
      coverageJSON: JSON.stringify(payload.coverageJSON ?? {}),
      inceptionDate: payload.inceptionDate,
    });
  }

  update(id: string, payload: Partial<Submission>) {
    return this.http.put<ApiResponse<Submission>>(`${this.base}/submissions/${id}`, payload);
  }

  updateStatus(id: string, status: string) {
    return this.http.patch<ApiResponse<Submission>>(`${this.base}/submissions/${id}/status`, { status });
  }

  // ── Questionnaire ─────────────────────────────────────────────────────────

  getQuestionnaire(submissionId: string) {
    return this.http.get<any>(`${this.base}/questionnaires/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : [res].filter(Boolean);
        return { data: normalizeQuestionnaire(items.length ? items[items.length - 1] : null) } as ApiResponse<Questionnaire>;
      })
    );
  }

  saveQuestionnaire(submissionId: string, responsesJSON: Record<string, unknown>) {
    return this.http.post<ApiResponse<Questionnaire>>(`${this.base}/questionnaires`, {
      submissionID:    submissionId,
      templateVersion: '1.0',
      responsesJSON:   JSON.stringify(responsesJSON),
      completedDate:   new Date().toISOString(),
    });
  }

  // ── Attachments ───────────────────────────────────────────────────────────

  getAttachments(submissionId: string) {
    return this.http.get<any>(`${this.base}/attachments/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeAttachment) } as ApiResponse<Attachment[]>;
      })
    );
  }

  uploadAttachment(submissionId: string, docType: string, file: File) {
    const uploadedBy = this.auth.currentUser()?.userId ?? 'system';
    return this.http.post<any>(`${this.base}/attachments`, {
      submissionID: submissionId,
      docType,
      fileURI:    `uploads/${submissionId}/${docType}/${file.name}`,
      uploadedBy,
    }).pipe(map(res => ({ data: normalizeAttachment(res?.data ?? res) } as ApiResponse<Attachment>)));
  }

  deleteAttachment(_submissionId: string, attachmentId: string) {
    return this.http.delete<ApiResponse<void>>(`${this.base}/attachments/${attachmentId}`);
  }

  // ── Completeness ──────────────────────────────────────────────────────────

  getCompletenessCheck(submissionId: string) {
    return this.http.get<any>(`${this.base}/completeness-checks/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: normalizeCompleteness(items.length ? items[items.length - 1] : null) } as ApiResponse<CompletenessCheck>;
      })
    );
  }

  runCompletenessCheck(submissionId: string) {
    return this.http.post<any>(`${this.base}/completeness-checks`, {
      submissionID:     submissionId,
      missingItemsJSON: '[]',
    }).pipe(map(res => ({ data: normalizeCompleteness(res?.data ?? res) } as ApiResponse<CompletenessCheck>)));
  }
}
