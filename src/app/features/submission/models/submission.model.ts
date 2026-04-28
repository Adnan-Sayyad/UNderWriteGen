export type ProductLine = 'Life' | 'Health' | 'PnC' | 'Commercial';
export type SubmissionStatus = 'Draft' | 'IntakeComplete' | 'UnderReview' | 'Quoted' | 'Declined' | 'Expired';
export type DocType = 'KYC' | 'Financial' | 'Medical' | 'Inspection' | 'Photos';
export type CheckStatus = 'Pending' | 'Complete';

export interface Submission {
  submissionId: string;
  partyId: string;
  agentId: string;
  productLine: ProductLine;
  coverageJSON: Record<string, unknown>;
  inceptionDate: string;
  createdDate: string;
  status: SubmissionStatus;
}

export interface Questionnaire {
  qId: string;
  submissionId: string;
  templateVersion: string;
  responsesJSON: Record<string, unknown>;
  completedDate: string;
}

export interface Attachment {
  attachmentId: string;
  submissionId: string;
  docType: DocType;
  fileUri: string;
  uploadedBy: string;
  uploadedDate: string;
}

export interface CompletenessCheck {
  checkId: string;
  submissionId: string;
  missingItemsJSON: string[];
  status: CheckStatus;
  checkedDate: string;
}
