import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { UWReport, ReportFilter } from '../models/reports.model';
import { ApiResponse, PagedResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly base = `${environment.apiBaseUrl}/reports`;

  constructor(private http: HttpClient) {}

  getReports(req: PageRequest, filter?: ReportFilter) {
    return this.http.get<PagedResponse<UWReport>>(this.base, { params: new HttpParams({ fromObject: { ...req, ...(filter ?? {}) } }) });
  }
  getReport(id: string) { return this.http.get<ApiResponse<UWReport>>(`${this.base}/${id}`); }
  generateReport(filter: ReportFilter) { return this.http.post<ApiResponse<UWReport>>(`${this.base}/generate`, filter); }
}
