import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { UWRule, RiskScore, ReferralMatrix, Referral } from '../models/rules.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class RulesApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  getRules(req: PageRequest) {
    return this.http.get<PagedResponse<UWRule>>(`${this.base}/uw-rules`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  createRule(payload: Partial<UWRule>) { return this.http.post<ApiResponse<UWRule>>(`${this.base}/uw-rules`, payload); }
  updateRule(id: string, payload: Partial<UWRule>) { return this.http.put<ApiResponse<UWRule>>(`${this.base}/uw-rules/${id}`, payload); }

  getRiskScore(submissionId: string) { return this.http.get<ApiResponse<RiskScore>>(`${this.base}/submissions/${submissionId}/risk-score`); }
  runScoring(submissionId: string) { return this.http.post<ApiResponse<RiskScore>>(`${this.base}/submissions/${submissionId}/risk-score/run`, {}); }

  getMatrices(productLine?: string) {
    const params = productLine ? new HttpParams().set('productLine', productLine) : undefined;
    return this.http.get<ApiResponse<ReferralMatrix[]>>(`${this.base}/referral-matrices`, { params });
  }

  getReferrals(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<PagedResponse<Referral>>(`${this.base}/referrals`, { params: new HttpParams({ fromObject: { ...req, ...filters } }) });
  }
  updateReferral(id: string, payload: Partial<Referral>) { return this.http.put<ApiResponse<Referral>>(`${this.base}/referrals/${id}`, payload); }
}
