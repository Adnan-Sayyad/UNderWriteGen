import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Submission, Attachment, CompletenessCheck, Questionnaire } from '../models/submission.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class SubmissionApiService {
  private readonly base = `${environment.apiBaseUrl}/submissions`;

  constructor(private http: HttpClient) {}

  getAll(req: PageRequest, filters?: Record<string, string>) {
    const params = new HttpParams({ fromObject: { ...req, ...filters } });
    return this.http.get<PagedResponse<Submission>>(this.base, { params });
  }
  getById(id: string) { return this.http.get<ApiResponse<Submission>>(`${this.base}/${id}`); }
  create(payload: Partial<Submission>) { return this.http.post<ApiResponse<Submission>>(this.base, payload); }
  update(id: string, payload: Partial<Submission>) { return this.http.put<ApiResponse<Submission>>(`${this.base}/${id}`, payload); }

  getQuestionnaire(submissionId: string) { return this.http.get<ApiResponse<Questionnaire>>(`${this.base}/${submissionId}/questionnaire`); }
  saveQuestionnaire(submissionId: string, payload: Partial<Questionnaire>) { return this.http.post<ApiResponse<Questionnaire>>(`${this.base}/${submissionId}/questionnaire`, payload); }

  getAttachments(submissionId: string) { return this.http.get<ApiResponse<Attachment[]>>(`${this.base}/${submissionId}/attachments`); }
  uploadAttachment(submissionId: string, form: FormData) { return this.http.post<ApiResponse<Attachment>>(`${this.base}/${submissionId}/attachments`, form); }
  deleteAttachment(submissionId: string, attachmentId: string) { return this.http.delete<ApiResponse<void>>(`${this.base}/${submissionId}/attachments/${attachmentId}`); }

  getCompletenessCheck(submissionId: string) { return this.http.get<ApiResponse<CompletenessCheck>>(`${this.base}/${submissionId}/completeness`); }
}
