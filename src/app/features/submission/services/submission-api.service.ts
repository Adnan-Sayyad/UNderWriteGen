import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Submission, Attachment, CompletenessCheck, Questionnaire, ProductLine, SubmissionStatus, Subjectivity, UWNote, RiskScore } from '../models/submission.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';
import { toPagedResponse } from '../../../shared/models/paging.util';
import { AuthService } from '../../../core/auth/auth.service';
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

function normalizeRiskScore(r: any): RiskScore {
  return {
    riskScoreId:  r.riskScoreId  ?? r.RiskScoreID  ?? r.riskScoreID  ?? '',
    submissionId: r.submissionId ?? r.SubmissionID  ?? '',
    modelVersion: r.modelVersion ?? r.ModelVersion  ?? '',
    scoreValue:   r.scoreValue   ?? r.ScoreValue    ?? 0,
    band:         r.band         ?? r.Band          ?? 'Low',
    scoredDate:   r.scoredDate   ?? r.ScoredDate    ?? new Date().toISOString(),
  };
}

function normalizeNote(r: any): UWNote {
  return {
    noteId:       r.noteId      ?? r.noteID      ?? r.NoteID      ?? '',
    submissionId: r.submissionId ?? r.submissionID ?? r.SubmissionID ?? '',
    authorId:     r.authorId    ?? r.authorID    ?? r.AuthorID    ?? '',
    noteText:     r.noteText    ?? r.NoteText    ?? '',
    createdDate:  r.createdDate ?? r.CreatedDate ?? new Date().toISOString(),
  };
}

function normalizeSubjectivity(r: any): Subjectivity {
  return {
    subjectivityId: r.subjectivityId ?? r.subjectivityID ?? r.SubjectivityID ?? '',
    submissionId:   r.submissionId   ?? r.submissionID   ?? r.SubmissionID   ?? '',
    description:    r.description    ?? r.Description    ?? '',
    dueDate:        r.dueDate        ?? r.DueDate        ?? '',
    status:         r.status         ?? r.Status         ?? 'Open',
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
      map(res => toPagedResponse<Submission>(res, req, normalizeSubmission))
    );
  }

  getById(id: string) {
    return this.http.get<any>(`${this.base}/submissions/${id}`).pipe(
      map(res => ({ data: normalizeSubmission(res?.data ?? res) } as ApiResponse<Submission>))
    );
  }

  create(payload: Partial<Submission>) {
    return this.http.post<any>(`${this.base}/submissions`, {
      partyID:       payload.partyId,
      agentID:       payload.agentId,
      productLine:   payload.productLine,
      coverageJSON:  JSON.stringify(payload.coverageJSON ?? {}),
      inceptionDate: payload.inceptionDate,
    }).pipe(map(res => ({ data: normalizeSubmission(res?.data ?? res) } as ApiResponse<Submission>)));
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

  runCompletenessCheck(submissionId: string, missingItems: string[] = []) {
    return this.http.post<any>(`${this.base}/completeness-checks`, {
      submissionID:     submissionId,
      missingItemsJSON: JSON.stringify(missingItems),
    }).pipe(map(res => ({ data: normalizeCompleteness(res?.data ?? res) } as ApiResponse<CompletenessCheck>)));
  }

  // ── Subjectivities ────────────────────────────────────────────────────────

  getSubjectivities(submissionId: string) {
    return this.http.get<any>(`${this.base}/subjectivities/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeSubjectivity) } as ApiResponse<Subjectivity[]>;
      })
    );
  }

  getAllSubjectivities() {
    return this.http.get<any>(`${this.base}/subjectivities`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeSubjectivity) } as ApiResponse<Subjectivity[]>;
      })
    );
  }

  createSubjectivity(submissionId: string, description: string, dueDate: string) {
    return this.http.post<any>(`${this.base}/subjectivities`, {
      submissionID: submissionId,
      description,
      dueDate,
    }).pipe(map(res => ({ data: normalizeSubjectivity(res?.data ?? res) } as ApiResponse<Subjectivity>)));
  }

  updateSubjectivityStatus(id: string, status: string) {
    return this.http.patch<any>(`${this.base}/subjectivities/${id}/status`, { status });
  }

  // ── UW Notes ──────────────────────────────────────────────────────────────

  getNotes(submissionId: string) {
    return this.http.get<any>(`${this.base}/uw-notes/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeNote) } as ApiResponse<UWNote[]>;
      })
    );
  }

  addNote(submissionId: string, noteText: string) {
    const authorId = this.auth.currentUser()?.userId ?? '00000000-0000-0000-0000-000000000000';
    return this.http.post<any>(`${this.base}/uw-notes`, {
      submissionID: submissionId,
      authorID:     authorId,
      noteText,
    }).pipe(map(res => ({ data: normalizeNote(res?.data ?? res) } as ApiResponse<UWNote>)));
  }

  deleteNote(noteId: string) {
    return this.http.delete<ApiResponse<void>>(`${this.base}/uw-notes/${noteId}`);
  }

  // ── Risk Score ─────────────────────────────────────────────────────────────

  getRiskScore(submissionId: string) {
    return this.http.get<any>(`${this.base}/risk-scores/${submissionId}`).pipe(
      map(res => ({ data: normalizeRiskScore(res?.data ?? res) } as ApiResponse<RiskScore>))
    );
  }

  calculateRiskScore(submissionId: string) {
    return this.http.post<any>(`${this.base}/risk-scores/calculate/${submissionId}`, {}).pipe(
      map(res => ({ data: normalizeRiskScore(res?.data ?? res) } as ApiResponse<RiskScore>))
    );
  }
}
