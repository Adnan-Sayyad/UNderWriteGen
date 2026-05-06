export type ReportScope = 'Product' | 'Region' | 'Agent' | 'Period';

export interface ReportSummaryDto {
  reportID: string;
  scope: string;
  scopeValue: string;
  generatedDate: string;
  periodStart: string | null;
  periodEnd: string | null;
  hitRatio: number;
  tAT_AvgHours: number;
  referralRate: number;
  avgPremium: number;
}

export interface ReportDetailDto extends ReportSummaryDto {
  generatedBy: string;
  quotes: number;
  boundPolicies: number;
  tAT_MinHours: number;
  tAT_MaxHours: number;
  totalReferrals: number;
  totalGrossPremium: number;
  riskMix: RiskMixDto;
  totalDecisions: number;
  avgDecisionsPerUW: number;
}

export interface GenerateReportRequest {
  scope: string;
  scopeValue: string;
  periodStart: string;
  periodEnd: string;
}

export interface HitRatioDto {
  scopeValue: string;
  quotes: number;
  boundPolicies: number;
  hitRatioPercent: number;
}

export interface TatDto {
  scopeValue: string;
  avgHours: number;
  minHours: number;
  maxHours: number;
}

export interface ReferralRateDto {
  scopeValue: string;
  quotes: number;
  totalReferrals: number;
  referralRatePercent: number;
}

export interface PremiumDistributionDto {
  scopeValue: string;
  totalGrossPremium: number;
  avgPremium: number;
  policyCount: number;
}

export interface RiskMixDto {
  scopeValue: string;
  high: number;
  medium: number;
  low: number;
  total: number;
}

export interface UWProductivityDto {
  scopeValue: string;
  totalDecisions: number;
  avgDecisionsPerUW: number;
}
