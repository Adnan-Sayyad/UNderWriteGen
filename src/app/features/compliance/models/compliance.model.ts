export type ChecklistStatus = 'Pending' | 'Complete';
export type BreachType = 'Authority' | 'RuleOverride' | 'PricingTolerance';
export type BreachStatus = 'Pending' | 'Approved' | 'Rejected';
export type ExceptionCategory = 'Data' | 'Process' | 'Compliance';
export type ExceptionStatus = 'Open' | 'Closed';

export interface ComplianceChecklist {
  checklistId: string;
  submissionId: string;
  itemsJSON: Array<{ item: string; checked: boolean }>;
  completedBy: string;
  completedDate: string;
  status: ChecklistStatus;
}

export interface AuthorityBreach {
  breachId: string;
  submissionId: string;
  breachType: BreachType;
  description: string;
  approvedBy: string;
  approvedDate: string;
  status: BreachStatus;
}

export interface ExceptionLog {
  exceptionId: string;
  submissionId: string;
  category: ExceptionCategory;
  details: string;
  loggedDate: string;
  status: ExceptionStatus;
}
