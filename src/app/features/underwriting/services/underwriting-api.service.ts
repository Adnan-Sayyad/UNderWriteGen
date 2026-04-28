import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { UWNote, UWDecision, Subjectivity } from '../models/underwriting.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class UnderwritingApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  getNotes(submissionId: string) { return this.http.get<ApiResponse<UWNote[]>>(`${this.base}/submissions/${submissionId}/notes`); }
  addNote(submissionId: string, payload: Pick<UWNote, 'noteText'>) { return this.http.post<ApiResponse<UWNote>>(`${this.base}/submissions/${submissionId}/notes`, payload); }

  getDecision(submissionId: string) { return this.http.get<ApiResponse<UWDecision>>(`${this.base}/submissions/${submissionId}/decision`); }
  submitDecision(submissionId: string, payload: Pick<UWDecision, 'decision' | 'reason'>) { return this.http.post<ApiResponse<UWDecision>>(`${this.base}/submissions/${submissionId}/decision`, payload); }

  getSubjectivities(req: PageRequest) {
    return this.http.get<PagedResponse<Subjectivity>>(`${this.base}/subjectivities`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  updateSubjectivity(id: string, payload: Partial<Subjectivity>) { return this.http.put<ApiResponse<Subjectivity>>(`${this.base}/subjectivities/${id}`, payload); }
}
