import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  UWRule, RiskScore,
  RuleSeverity, UWStatus, RiskBand,
  EvaluateRulesResponse, RuleEvaluationResult, PagedResult,
  SEVERITIES, RISK_BANDS, UW_STATUSES,
  ConditionOperator,
} from '../models/rules.model';

function normalizePage<T>(res: any, normalizer: (x: any) => T): PagedResult<T> {
  if (Array.isArray(res)) {
    const items = res.map(normalizer);
    return { content: items, page: 0, size: items.length || 1, totalElements: items.length, totalPages: 1 };
  }
  const items = Array.isArray(res?.content) ? res.content.map(normalizer) : [];
  return {
    content:       items,
    page:          Number(res?.page          ?? 0),
    size:          Number(res?.size          ?? items.length),
    totalElements: Number(res?.totalElements ?? items.length),
    totalPages:    Number(res?.totalPages    ?? (items.length ? 1 : 0)),
  };
}

function pageParams(page: number, size: number, extra?: Record<string, string | number | undefined | null>): HttpParams {
  let p = new HttpParams().set('page', String(page)).set('size', String(size));
  if (extra) {
    for (const [k, v] of Object.entries(extra)) {
      if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v));
    }
  }
  return p;
}

function toEnumString<T extends string>(value: unknown, table: readonly T[], fallback: T): T {
  if (typeof value === 'number' && value >= 0 && value < table.length) return table[value];
  if (typeof value === 'string' && (table as readonly string[]).includes(value)) return value as T;
  return fallback;
}

function pickId(raw: any, ...keys: string[]): string {
  for (const k of keys) {
    const v = raw?.[k] ?? raw?.[k.charAt(0).toLowerCase() + k.slice(1)] ?? raw?.[k.toUpperCase()];
    if (v) return String(v);
  }
  return '';
}

function normalizeUWRule(r: any): UWRule {
  return {
    uwRuleID:       pickId(r, 'uwRuleID', 'UWRuleID', 'ruleId', 'RuleID'),
    productLine:    r?.productLine ?? r?.ProductLine ?? '',
    ruleName:       (r?.ruleName ?? r?.RuleName ?? null) || null,
    description:    (r?.description ?? r?.Description ?? null) || null,
    expressionJSON: r?.expressionJSON ?? r?.ExpressionJSON ?? '',
    severity:       toEnumString<RuleSeverity>(r?.severity ?? r?.Severity, SEVERITIES, 'Info'),
    status:         toEnumString<UWStatus>(r?.status ?? r?.Status, UW_STATUSES, 'Active'),
  };
}

function normalizeRiskScore(r: any): RiskScore {
  return {
    riskScoreID:  pickId(r, 'riskScoreID', 'RiskScoreID', 'scoreId'),
    submissionID: pickId(r, 'submissionID', 'SubmissionID', 'submissionId'),
    modelVersion: r?.modelVersion ?? r?.ModelVersion ?? '',
    scoreValue:   Number(r?.scoreValue ?? r?.ScoreValue ?? 0),
    band:         toEnumString<RiskBand>(r?.band ?? r?.Band, RISK_BANDS, 'Low'),
    scoredDate:   r?.scoredDate ?? r?.ScoredDate ?? '',
  };
}

function asArray(res: any): any[] {
  if (Array.isArray(res)) return res;
  if (Array.isArray(res?.data)) return res.data;
  if (Array.isArray(res?.content)) return res.content;
  return [];
}

@Injectable({ providedIn: 'root' })
export class RulesApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // ── UW Rules ──────────────────────────────────────────────────────────────
  getRules(page = 0, size = 20, filters?: {
    productLine?: string; severity?: RuleSeverity | ''; status?: UWStatus | '';
  }): Observable<PagedResult<UWRule>> {
    const params = pageParams(page, size, {
      productLine: filters?.productLine || undefined,
      severity:    filters?.severity    || undefined,
      status:      filters?.status      || undefined,
    });
    return this.http.get<any>(`${this.base}/uw-rules`, { params })
      .pipe(map(r => normalizePage(r, normalizeUWRule)));
  }

  getRuleById(id: string): Observable<UWRule | null> {
    return this.http.get<any>(`${this.base}/uw-rules/${id}`)
      .pipe(map(r => r ? normalizeUWRule(r?.data ?? r) : null));
  }

  getRulesByProductLine(line: string): Observable<UWRule[]> {
    return this.http.get<any>(`${this.base}/uw-rules/product-line/${encodeURIComponent(line)}`)
      .pipe(map(r => asArray(r).map(normalizeUWRule)));
  }

  getRulesBySeverity(severity: RuleSeverity): Observable<UWRule[]> {
    return this.http.get<any>(`${this.base}/uw-rules/severity/${severity}`)
      .pipe(map(r => asArray(r).map(normalizeUWRule)));
  }

  createRule(payload: {
    productLine: string;
    ruleName?: string | null;
    description?: string | null;
    expressionJSON: string;
    severity: RuleSeverity;
    status: UWStatus;
  }) {
    return this.http.post<any>(`${this.base}/uw-rules`, payload)
      .pipe(map(r => normalizeUWRule(r?.data ?? r)));
  }

  updateRule(id: string, payload: {
    productLine: string;
    ruleName?: string | null;
    description?: string | null;
    expressionJSON: string;
    severity: RuleSeverity;
    status: UWStatus;
  }) {
    return this.http.put<any>(`${this.base}/uw-rules/${id}`, payload)
      .pipe(map(r => normalizeUWRule(r?.data ?? r)));
  }

  updateRuleStatus(id: string, status: UWStatus) {
    return this.http.patch<any>(`${this.base}/uw-rules/${id}/status`, { status })
      .pipe(map(r => normalizeUWRule(r?.data ?? r)));
  }

  deleteRule(id: string) {
    return this.http.delete<void>(`${this.base}/uw-rules/${id}`);
  }

  evaluateRules(submissionId: string): Observable<EvaluateRulesResponse> {
    return this.http.post<any>(`${this.base}/uw-rules/evaluate/${submissionId}`, {})
      .pipe(map(r => {
        const data = r?.data ?? r;
        return {
          submissionID: pickId(data, 'submissionID', 'SubmissionID'),
          evaluatedAt:  data?.evaluatedAt ?? data?.EvaluatedAt ?? '',
          results: asArray({ data: data?.results ?? data?.Results }).map((x): RuleEvaluationResult => ({
            uwRuleID:    pickId(x, 'uwRuleID', 'UWRuleID'),
            ruleName:    (x?.ruleName ?? x?.RuleName ?? null) || null,
            productLine: x?.productLine ?? x?.ProductLine ?? '',
            severity:    toEnumString<RuleSeverity>(x?.severity ?? x?.Severity, SEVERITIES, 'Info'),
            triggered:   !!(x?.triggered ?? x?.Triggered),
            message:     x?.message ?? x?.Message ?? '',
          })),
        };
      }));
  }

  // ── Risk Scores ───────────────────────────────────────────────────────────
  getLatestRiskScore(submissionId: string): Observable<RiskScore | null> {
    return this.http.get<any>(`${this.base}/risk-scores/${submissionId}`)
      .pipe(map(r => r ? normalizeRiskScore(r?.data ?? r) : null));
  }

  getRiskScoreHistory(submissionId: string, page = 0, size = 20): Observable<PagedResult<RiskScore>> {
    const params = pageParams(page, size);
    return this.http.get<any>(`${this.base}/risk-scores/${submissionId}/history`, { params })
      .pipe(map(r => normalizePage(r, normalizeRiskScore)));
  }

  getScoresByBand(band: RiskBand, page = 0, size = 20): Observable<PagedResult<RiskScore>> {
    const params = pageParams(page, size);
    return this.http.get<any>(`${this.base}/risk-scores/band/${band}`, { params })
      .pipe(map(r => normalizePage(r, normalizeRiskScore)));
  }

  calculateRiskScore(submissionId: string): Observable<RiskScore> {
    return this.http.post<any>(`${this.base}/risk-scores/calculate/${submissionId}`, {})
      .pipe(map(r => normalizeRiskScore(r?.data ?? r)));
  }
}
