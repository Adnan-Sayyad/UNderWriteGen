export type DecisionType = 'Approve' | 'Decline' | 'Refer' | 'MoreInfo';
export type SubjectivityStatus = 'Open' | 'Met' | 'Waived';

export interface UWNote {
  noteId: string;
  submissionId: string;
  authorId: string;
  noteText: string;
  createdDate: string;
}

export interface UWDecision {
  decisionId: string;
  submissionId: string;
  decision: DecisionType;
  reason: string;
  decidedBy: string;
  decidedDate: string;
}

export interface Subjectivity {
  subjectivityId: string;
  submissionId: string;
  description: string;
  dueDate: string;
  status: SubjectivityStatus;
}
