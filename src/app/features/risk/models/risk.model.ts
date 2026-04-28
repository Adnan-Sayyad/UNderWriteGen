export type RiskType = 'Life' | 'Health' | 'Property' | 'Auto' | 'Marine' | 'GL';
export type EvidenceType = 'InspectionReport' | 'Medical' | 'Lab' | 'Telematics' | 'ClaimsHistory' | 'Sanctions';
export type EvidenceStatus = 'Requested' | 'Received' | 'NotAvailable';

export interface RiskProfile {
  riskId: string;
  submissionId: string;
  riskType: RiskType;
  attributesJSON: Record<string, unknown>;
  riskNotes: string;
  lastUpdated: string;
}

export interface EvidenceRef {
  evidenceId: string;
  submissionId: string;
  evidenceType: EvidenceType;
  provider: string;
  referenceNo: string;
  resultJSON: Record<string, unknown>;
  receivedDate: string;
  status: EvidenceStatus;
}
