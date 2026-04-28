import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Policy, Endorsement, Cancellation, Renewal } from '../models/policy.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class PolicyApiService {
  private readonly base = `${environment.apiBaseUrl}/policies`;

  constructor(private http: HttpClient) {}

  getAll(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<PagedResponse<Policy>>(this.base, { params: new HttpParams({ fromObject: { ...req, ...filters } }) });
  }
  getById(id: string) { return this.http.get<ApiResponse<Policy>>(`${this.base}/${id}`); }
  bind(submissionId: string) { return this.http.post<ApiResponse<Policy>>(`${this.base}/bind`, { submissionId }); }

  createEndorsement(policyId: string, payload: Partial<Endorsement>) { return this.http.post<ApiResponse<Endorsement>>(`${this.base}/${policyId}/endorsements`, payload); }
  approveEndorsement(id: string) { return this.http.post<ApiResponse<Endorsement>>(`${environment.apiBaseUrl}/endorsements/${id}/approve`, {}); }

  requestCancellation(policyId: string, payload: Partial<Cancellation>) { return this.http.post<ApiResponse<Cancellation>>(`${this.base}/${policyId}/cancellations`, payload); }

  getRenewals(req: PageRequest) {
    return this.http.get<PagedResponse<Renewal>>(`${environment.apiBaseUrl}/renewals`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  updateRenewal(id: string, payload: Partial<Renewal>) { return this.http.put<ApiResponse<Renewal>>(`${environment.apiBaseUrl}/renewals/${id}`, payload); }
}
