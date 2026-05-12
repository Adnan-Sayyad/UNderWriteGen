import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { UWNote, UWDecision, Subjectivity } from '../models/underwriting.model';
import { ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';
import { toPagedResponse } from '../../../shared/models/paging.util';
import { AuthService } from '../../../core/auth/auth.service';

const NULL_GUID = '00000000-0000-0000-0000-000000000000';

function normalizeNote(r: any): UWNote {
  return {
    noteId:       r.noteId       ?? r.noteID       ?? r.NoteID       ?? '',
    submissionId: r.submissionId ?? r.submissionID ?? r.SubmissionID ?? '',
    authorId:     r.authorId     ?? r.authorID     ?? r.AuthorID     ?? '',
    noteText:     r.noteText     ?? r.NoteText     ?? '',
    createdDate:  r.createdDate  ?? r.CreatedDate  ?? new Date().toISOString(),
  };
}

function normalizeDecision(r: any): UWDecision {
  return {
    decisionId:   r.decisionId   ?? r.decisionID   ?? r.DecisionID   ?? '',
    submissionId: r.submissionId ?? r.submissionID ?? r.SubmissionID ?? '',
    decision:     r.decision     ?? r.Decision     ?? 'Approve',
    reason:       r.reason       ?? r.Reason       ?? '',
    decidedBy:    r.decidedBy    ?? r.DecidedBy    ?? '',
    decidedDate:  r.decidedDate  ?? r.DecidedDate  ?? new Date().toISOString(),
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

@Injectable({ providedIn: 'root' })
export class UnderwritingApiService {
  private readonly base = `${environment.apiBaseUrl}`;
  private readonly auth = inject(AuthService);

  constructor(private http: HttpClient) {}

  getNotes(submissionId: string) {
    return this.http.get<any>(`${this.base}/uw-notes/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.map(normalizeNote) } as ApiResponse<UWNote[]>;
      })
    );
  }

  addNote(submissionId: string, payload: Pick<UWNote, 'noteText'>) {
    const userId = this.auth.currentUser()?.userId ?? NULL_GUID;
    return this.http.post<ApiResponse<UWNote>>(`${this.base}/uw-notes`, {
      submissionID: submissionId,
      authorID: userId,
      noteText: payload.noteText,
    });
  }

  getDecision(submissionId: string) {
    return this.http.get<any>(`${this.base}/uw-decisions/${submissionId}`).pipe(
      map(res => {
        const items: any[] = Array.isArray(res) ? res : (res?.data ?? []);
        return { data: items.length > 0 ? normalizeDecision(items[0]) : null } as ApiResponse<UWDecision>;
      })
    );
  }

  submitDecision(submissionId: string, payload: Pick<UWDecision, 'decision' | 'reason'>) {
    const userId = this.auth.currentUser()?.userId ?? NULL_GUID;
    return this.http.post<ApiResponse<UWDecision>>(`${this.base}/uw-decisions`, {
      submissionID: submissionId,
      decision: payload.decision,
      reason: payload.reason,
      decidedBy: userId,
    });
  }

  deleteNote(_submissionId: string, noteId: string) {
    return this.http.delete<ApiResponse<void>>(`${this.base}/uw-notes/${noteId}`);
  }

  getSubjectivities(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<any>(`${this.base}/subjectivities`, {
      params: new HttpParams({ fromObject: { ...req, ...filters } as any }),
    }).pipe(map(res => toPagedResponse<Subjectivity>(res, req, normalizeSubjectivity)));
  }

  createSubjectivity(payload: Omit<Subjectivity, 'subjectivityId'>) {
    return this.http.post<ApiResponse<Subjectivity>>(`${this.base}/subjectivities`, payload);
  }

  updateSubjectivity(id: string, payload: Partial<Subjectivity>) {
    return this.http.put<ApiResponse<Subjectivity>>(`${this.base}/subjectivities/${id}`, payload);
  }
}
