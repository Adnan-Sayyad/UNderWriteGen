import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { RiskProfile, EvidenceRef } from '../models/risk.model';
import { ApiResponse } from '../../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class RiskApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  getRiskProfile(submissionId: string) { return this.http.get<ApiResponse<RiskProfile>>(`${this.base}/submissions/${submissionId}/risk-profile`); }
  saveRiskProfile(submissionId: string, payload: Partial<RiskProfile>) { return this.http.post<ApiResponse<RiskProfile>>(`${this.base}/submissions/${submissionId}/risk-profile`, payload); }

  getEvidence(submissionId: string) { return this.http.get<ApiResponse<EvidenceRef[]>>(`${this.base}/submissions/${submissionId}/evidence`); }
  addEvidence(submissionId: string, payload: Partial<EvidenceRef>) { return this.http.post<ApiResponse<EvidenceRef>>(`${this.base}/submissions/${submissionId}/evidence`, payload); }
  updateEvidence(id: string, payload: Partial<EvidenceRef>) { return this.http.put<ApiResponse<EvidenceRef>>(`${this.base}/evidence/${id}`, payload); }
}
