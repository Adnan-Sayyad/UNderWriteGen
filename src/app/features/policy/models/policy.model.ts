export type PolicyStatus = 'Active' | 'Cancelled' | 'Expired';
export type RenewalStatus = 'Offered' | 'Accepted' | 'Declined';

export interface Policy {
  policyId: string;
  submissionId: string;
  policyNumber: string;
  productLine: string;
  coverageJSON: Record<string, unknown>;
  inceptionDate: string;
  expiryDate: string;
  status: PolicyStatus;
}

export interface Renewal {
  renewalId: string;
  policyId: string;
  renewalOfferJSON: Record<string, unknown>;
  offeredDate: string;
  status: RenewalStatus;
}
