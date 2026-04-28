import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ComplianceChecklist, AuthorityBreach, ExceptionLog } from '../models/compliance.model';
import { AuditLog } from '../../auth/models/user.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class ComplianceApiService {
  private readonly base = `${environment.apiBaseUrl}`;

  constructor(private http: HttpClient) {}

  getChecklists(req: PageRequest) {
    return this.http.get<PagedResponse<ComplianceChecklist>>(`${this.base}/compliance-checklists`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  updateChecklist(id: string, payload: Partial<ComplianceChecklist>) { return this.http.put<ApiResponse<ComplianceChecklist>>(`${this.base}/compliance-checklists/${id}`, payload); }

  getBreaches(req: PageRequest) {
    return this.http.get<PagedResponse<AuthorityBreach>>(`${this.base}/authority-breaches`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  approveBreachException(id: string) { return this.http.post<ApiResponse<AuthorityBreach>>(`${this.base}/authority-breaches/${id}/approve`, {}); }

  getExceptions(req: PageRequest) {
    return this.http.get<PagedResponse<ExceptionLog>>(`${this.base}/exception-logs`, { params: new HttpParams({ fromObject: { ...req } }) });
  }
  closeException(id: string) { return this.http.post<ApiResponse<ExceptionLog>>(`${this.base}/exception-logs/${id}/close`, {}); }

  getAuditLogs(req: PageRequest, filters?: Record<string, string>) {
    return this.http.get<PagedResponse<AuditLog>>(`${this.base}/audit-logs`, { params: new HttpParams({ fromObject: { ...req, ...filters } }) });
  }
}
