import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import {
  ReportSummaryDto,
  ReportDetailDto,
  GenerateReportRequest,
  HitRatioDto,
  TatDto,
  ReferralRateDto,
  PremiumDistributionDto,
  RiskMixDto,
  UWProductivityDto,
} from '../models/reports.model';

@Injectable({ providedIn: 'root' })
export class ReportsApiService {
  private readonly base = `${environment.apiBaseUrl}/reports`;

  constructor(private http: HttpClient) {}

  // Backend uses 1-based page index and pageSize param
  getReports(page = 1, pageSize = 20) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ReportSummaryDto[]>(this.base, { params });
  }

  getReportsByScope(scope: string) {
    return this.http.get<ReportSummaryDto[]>(`${this.base}/scope/${scope}`);
  }

  getReport(id: string) {
    return this.http.get<ReportDetailDto>(`${this.base}/${id}`);
  }

  generateReport(req: GenerateReportRequest) {
    return this.http.post<ReportDetailDto>(`${this.base}/generate`, req);
  }

  private metricParams(scope?: string, from?: string, to?: string): HttpParams {
    let p = new HttpParams();
    if (scope) p = p.set('scope', scope);
    if (from)  p = p.set('from', from);
    if (to)    p = p.set('to', to);
    return p;
  }

  getHitRatio(scope?: string, from?: string, to?: string) {
    return this.http.get<HitRatioDto[]>(`${this.base}/metrics/hit-ratio`,
      { params: this.metricParams(scope, from, to) });
  }

  getTat(scope?: string, from?: string, to?: string) {
    return this.http.get<TatDto[]>(`${this.base}/metrics/tat`,
      { params: this.metricParams(scope, from, to) });
  }

  getReferralRate(scope?: string, from?: string, to?: string) {
    return this.http.get<ReferralRateDto[]>(`${this.base}/metrics/referral-rate`,
      { params: this.metricParams(scope, from, to) });
  }

  getPremiumDistribution(scope?: string, from?: string, to?: string) {
    return this.http.get<PremiumDistributionDto[]>(`${this.base}/metrics/premium-distribution`,
      { params: this.metricParams(scope, from, to) });
  }

  getRiskMix(scope?: string, from?: string, to?: string) {
    return this.http.get<RiskMixDto[]>(`${this.base}/metrics/risk-mix`,
      { params: this.metricParams(scope, from, to) });
  }

  getUWProductivity(scope?: string, from?: string, to?: string) {
    return this.http.get<UWProductivityDto[]>(`${this.base}/metrics/uw-productivity`,
      { params: this.metricParams(scope, from, to) });
  }
}
