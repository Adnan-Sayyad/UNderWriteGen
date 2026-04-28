export type PolicyStatus = 'Active' | 'Cancelled' | 'Expired';
export type EndorsementType = 'MidTermChange' | 'Address' | 'Limit' | 'Deductible' | 'Beneficiary';
export type EndorsementStatus = 'Proposed' | 'Approved' | 'Posted';
export type CancellationStatus = 'Requested' | 'Approved' | 'Posted';
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

export interface Endorsement {
  endorsementId: string;
  policyId: string;
  endorsementType: EndorsementType;
  changesJSON: Record<string, unknown>;
  effectiveDate: string;
  premiumDelta: number;
  status: EndorsementStatus;
}

export interface Cancellation {
  cancellationId: string;
  policyId: string;
  cancelReason: string;
  cancelDate: string;
  refundPremium: number;
  status: CancellationStatus;
}

export interface Renewal {
  renewalId: string;
  policyId: string;
  renewalOfferJSON: Record<string, unknown>;
  offeredDate: string;
  status: RenewalStatus;
}
