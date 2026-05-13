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

export type RiskBand = 'Low' | 'Medium' | 'High';

export interface RiskScore {
  riskScoreId:  string;
  submissionId: string;
  modelVersion: string;
  scoreValue:   number;
  band:         RiskBand;
  scoredDate:   string;
}

export interface UWNote {
  noteId:       string;
  submissionId: string;
  authorId:     string;
  noteText:     string;
  createdDate:  string;
}

export type SubjectivityStatus = 'Open' | 'Met' | 'Waived';

export interface Subjectivity {
  subjectivityId: string;
  submissionId:   string;
  description:    string;
  dueDate:        string;
  status:         SubjectivityStatus;
}
