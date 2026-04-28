export type ReportScope = 'Product' | 'Region' | 'Agent' | 'Period';

export interface UWReport {
  reportId: string;
  scope: ReportScope;
  metrics: {
    quotes: number;
    hitRatio: number;
    tat: number;
    referralRate: number;
    avgPremium: number;
    riskMix: Record<string, number>;
  };
  generatedDate: string;
}

export interface ReportFilter {
  scope?: ReportScope;
  productLine?: string;
  region?: string;
  agentId?: string;
  fromDate?: string;
  toDate?: string;
}
