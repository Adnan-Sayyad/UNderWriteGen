import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  UWRule, RiskScore, ReferralMatrix, Referral,
  RuleSeverity, UWStatus, RiskBand, Authority, ReferralStatus, CriteriaType,
  EvaluateRulesResponse, RuleEvaluationResult,
  SEVERITIES, RISK_BANDS, AUTHORITIES, REFERRAL_STATUSES, CRITERIA_TYPES, UW_STATUSES,
  ConditionOperator,
} from '../models/rules.model';

// ── enum normalisation: backend may send numeric or string enums ────────────
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

function normalizeMatrix(r: any): ReferralMatrix {
  const opRaw = (r?.operator ?? r?.Operator ?? null);
  return {
    referralMatrixID: pickId(r, 'referralMatrixID', 'ReferralMatrixID', 'matrixId'),
    productLine:      r?.productLine ?? r?.ProductLine ?? '',
    criteriaJSON:     toEnumString<CriteriaType>(r?.criteriaJSON ?? r?.CriteriaJSON, CRITERIA_TYPES, 'SumInsured'),
    operator:         (opRaw ?? null) as (ConditionOperator | null),
    threshold:        (r?.threshold ?? r?.Threshold ?? null) || null,
    requiredAuthority:toEnumString<Authority>(r?.requiredAuthority ?? r?.RequiredAuthority, AUTHORITIES, 'UW1'),
    status:           toEnumString<UWStatus>(r?.status ?? r?.Status, UW_STATUSES, 'Active'),
  };
}

function normalizeReferral(r: any): Referral {
  return {
    referralID:        pickId(r, 'referralID', 'ReferralID', 'referralId'),
    submissionID:      pickId(r, 'submissionID', 'SubmissionID', 'submissionId'),
    raisedBy:          r?.raisedBy ?? r?.RaisedBy ?? '',
    reason:            r?.reason ?? r?.Reason ?? '',
    requiredAuthority: toEnumString<Authority>(r?.requiredAuthority ?? r?.RequiredAuthority, AUTHORITIES, 'UW1'),
    assignedTo:        r?.assignedTo ?? r?.AssignedTo ?? '',
    createdDate:       r?.createdDate ?? r?.CreatedDate ?? '',
    status:            toEnumString<ReferralStatus>(r?.status ?? r?.Status, REFERRAL_STATUSES, 'Pending'),
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
  getRules(): Observable<UWRule[]> {
    return this.http.get<any>(`${this.base}/uw-rules`)
      .pipe(map(r => asArray(r).map(normalizeUWRule)));
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
          submissionID:     pickId(data, 'submissionID', 'SubmissionID'),
          hasBlockingRules: !!(data?.hasBlockingRules ?? data?.HasBlockingRules),
          evaluatedAt:      data?.evaluatedAt ?? data?.EvaluatedAt ?? '',
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

  // ── Referral Matrix ───────────────────────────────────────────────────────
  getMatrices(): Observable<ReferralMatrix[]> {
    return this.http.get<any>(`${this.base}/referral-matrix`)
      .pipe(map(r => asArray(r).map(normalizeMatrix)));
  }

  getMatrixById(id: string): Observable<ReferralMatrix | null> {
    return this.http.get<any>(`${this.base}/referral-matrix/${id}`)
      .pipe(map(r => r ? normalizeMatrix(r?.data ?? r) : null));
  }

  createMatrix(payload: {
    productLine: string;
    criteriaJSON: CriteriaType;
    operator?: ConditionOperator | null;
    threshold?: string | null;
    requiredAuthority: Authority;
    status: UWStatus;
  }) {
    return this.http.post<any>(`${this.base}/referral-matrix`, payload)
      .pipe(map(r => normalizeMatrix(r?.data ?? r)));
  }

  updateMatrix(id: string, payload: {
    productLine: string;
    criteriaJSON: CriteriaType;
    operator?: ConditionOperator | null;
    threshold?: string | null;
    requiredAuthority: Authority;
    status: UWStatus;
  }) {
    return this.http.put<any>(`${this.base}/referral-matrix/${id}`, payload)
      .pipe(map(r => normalizeMatrix(r?.data ?? r)));
  }

  updateMatrixStatus(id: string, status: UWStatus) {
    return this.http.patch<any>(`${this.base}/referral-matrix/${id}/status`, { status })
      .pipe(map(r => normalizeMatrix(r?.data ?? r)));
  }

  deleteMatrix(id: string) {
    return this.http.delete<void>(`${this.base}/referral-matrix/${id}`);
  }

  // ── Referrals ─────────────────────────────────────────────────────────────
  getReferrals(): Observable<Referral[]> {
    return this.http.get<any>(`${this.base}/referrals`)
      .pipe(map(r => asArray(r).map(normalizeReferral)));
  }

  getReferralById(id: string): Observable<Referral | null> {
    return this.http.get<any>(`${this.base}/referrals/${id}`)
      .pipe(map(r => r ? normalizeReferral(r?.data ?? r) : null));
  }

  getReferralsBySubmission(submissionId: string): Observable<Referral[]> {
    return this.http.get<any>(`${this.base}/referrals/submission/${submissionId}`)
      .pipe(map(r => asArray(r).map(normalizeReferral)));
  }

  getReferralsByAuthority(authority: Authority): Observable<Referral[]> {
    return this.http.get<any>(`${this.base}/referrals/authority/${authority}`)
      .pipe(map(r => asArray(r).map(normalizeReferral)));
  }

  getReferralsAssignedTo(userId: string): Observable<Referral[]> {
    return this.http.get<any>(`${this.base}/referrals/assigned/${encodeURIComponent(userId)}`)
      .pipe(map(r => asArray(r).map(normalizeReferral)));
  }

  createReferral(payload: { submissionID: string; raisedBy: string; reason: string; requiredAuthority: Authority; assignedTo: string; }) {
    return this.http.post<any>(`${this.base}/referrals`, payload)
      .pipe(map(r => normalizeReferral(r?.data ?? r)));
  }

  updateReferral(id: string, payload: { assignedTo: string; status: ReferralStatus; }) {
    return this.http.put<any>(`${this.base}/referrals/${id}`, payload)
      .pipe(map(r => normalizeReferral(r?.data ?? r)));
  }

  updateReferralStatus(id: string, status: ReferralStatus) {
    return this.http.patch<any>(`${this.base}/referrals/${id}/status`, { status })
      .pipe(map(r => normalizeReferral(r?.data ?? r)));
  }

  // ── Risk Scores ───────────────────────────────────────────────────────────
  getLatestRiskScore(submissionId: string): Observable<RiskScore | null> {
    return this.http.get<any>(`${this.base}/risk-scores/${submissionId}`)
      .pipe(map(r => r ? normalizeRiskScore(r?.data ?? r) : null));
  }

  getRiskScoreHistory(submissionId: string): Observable<RiskScore[]> {
    return this.http.get<any>(`${this.base}/risk-scores/${submissionId}/history`)
      .pipe(map(r => asArray(r).map(normalizeRiskScore)));
  }

  getScoresByBand(band: RiskBand): Observable<RiskScore[]> {
    return this.http.get<any>(`${this.base}/risk-scores/band/${band}`)
      .pipe(map(r => asArray(r).map(normalizeRiskScore)));
  }

  calculateRiskScore(submissionId: string): Observable<RiskScore> {
    return this.http.post<any>(`${this.base}/risk-scores/calculate/${submissionId}`, {})
      .pipe(map(r => normalizeRiskScore(r?.data ?? r)));
  }
}
