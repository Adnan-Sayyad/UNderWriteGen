export type UserRole = 'Agent' | 'Underwriter' | 'Assistant' | 'Pricing' | 'Compliance' | 'Operations' | 'Admin';
export type UserStatus = 'Active' | 'Locked' | 'Disabled';

export interface User {
  userId: string;
  name: string;
  role: UserRole;
  email: string;
  phone: string;
  status: UserStatus;
}

export interface AuditLog {
  auditId: string;
  userId: string;
  action: string;
  resource: string;
  timestamp: string;
  metadata?: Record<string, unknown>;
}
