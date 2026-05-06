// ── Compliance Checklist ──────────────────────────────────────────
export type ChecklistStatus = 'Pending' | 'InProgress' | 'Completed';

export interface ComplianceChecklist {
  checklistId:   string;
  submissionId:  string;
  itemsJson:     string;          // JSON string stored as-is from backend
  completedBy?:  string;
  completedDate?: string;
  status:        ChecklistStatus;
  createdAt:     string;
  updatedAt?:    string;
}

export interface CreateChecklistPayload {
  submissionId:  string;
  itemsJson:     string;
  completedBy?:  string;
  completedDate?: string;
  status:        ChecklistStatus;
}

export interface UpdateChecklistPayload {
  itemsJson:     string;
  completedBy?:  string;
  completedDate?: string;
}

export interface UpdateChecklistStatusPayload { status: ChecklistStatus; }

// ── Authority Breach ──────────────────────────────────────────────
export type BreachType   = 'Authority' | 'RuleOverride' | 'PricingTolerance';
export type BreachStatus = 'Pending' | 'Approved' | 'Rejected';

export interface AuthorityBreach {
  breachId:     string;
  submissionId: string;
  breachType:   BreachType;
  description:  string;
  approvedBy?:  string;
  approvedDate?: string;
  status:       BreachStatus;
  createdAt:    string;
  updatedAt?:   string;
}

export interface CreateBreachPayload {
  submissionId: string;
  breachType:   BreachType;
  description:  string;
}

export interface UpdateBreachStatusPayload {
  status:      BreachStatus;
  approvedBy?: string;
  approvedDate?: string;
}

// ── Exception Log ─────────────────────────────────────────────────
export type ExceptionCategory = 'Data' | 'Process' | 'Compliance';
export type ExceptionStatus   = 'Open' | 'Closed';

export interface ExceptionLog {
  exceptionId:  string;
  submissionId: string;
  category:     ExceptionCategory;
  details:      string;
  loggedDate:   string;
  status:       ExceptionStatus;
  createdAt:    string;
  updatedAt?:   string;
}

export interface CreateExceptionPayload {
  submissionId: string;
  category:     ExceptionCategory;
  details:      string;
}

export interface UpdateExceptionStatusPayload { status: ExceptionStatus; }
