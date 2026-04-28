export type RuleSeverity = 'Block' | 'Refer' | 'Load' | 'Info';
export type RuleStatus = 'Active' | 'Inactive';
export type RiskBand = 'Low' | 'Medium' | 'High';
export type Authority = 'UW1' | 'UW2' | 'UWManager' | 'Committee';
export type ReferralStatus = 'Pending' | 'Approved' | 'Rejected';

export interface UWRule {
  ruleId: string;
  productLine: string;
  expressionJSON: Record<string, unknown>;
  severity: RuleSeverity;
  status: RuleStatus;
}

export interface RiskScore {
  scoreId: string;
  submissionId: string;
  modelVersion: string;
  scoreValue: number;
  band: RiskBand;
  scoredDate: string;
}

export interface ReferralMatrix {
  matrixId: string;
  productLine: string;
  criteriaJSON: { sumInsured?: number; class?: string; riskBand?: RiskBand };
  requiredAuthority: Authority;
  status: RuleStatus;
}

export interface Referral {
  referralId: string;
  submissionId: string;
  raisedBy: string;
  reason: string;
  requiredAuthority: Authority;
  assignedTo: string;
  createdDate: string;
  status: ReferralStatus;
}
