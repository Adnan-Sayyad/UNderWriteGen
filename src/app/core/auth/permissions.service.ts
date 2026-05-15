import { Injectable, computed } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Central permissions service.
 * All role-based feature flags live here so components stay thin.
 *
 * Role matrix:
 *  Agent          – Submissions (own), Agents (view), Customer Parties (own)
 *  Underwriter    – UW Workbench, Risk & Evidence, Rules & Scoring (view only)
 *  UWAssistant    – Risk & Evidence, Rules & Scoring (view only), Checklist
 *  PricingAnalyst – Create Quote, Quotes, Parameters
 *  Operations     – Rules & Scoring (full edit), Policy Desk
 *  Compliance     – Checklists, Authority Breaches, Exception Log, Reports
 *  Admin          – All access; Submissions view only, Rules view only
 */
@Injectable({ providedIn: 'root' })
export class PermissionsService {
  constructor(private auth: AuthService) {}

  // ── Convenience role checks ──────────────────────────────────────────────

  readonly isAgent          = computed(() => this.auth.hasRole('Agent'));
  readonly isUnderwriter    = computed(() => this.auth.hasRole('Underwriter'));
  readonly isUWAssistant    = computed(() => this.auth.hasRole('UWAssistant'));
  readonly isPricingAnalyst = computed(() => this.auth.hasRole('PricingAnalyst'));
  readonly isOperations     = computed(() => this.auth.hasRole('Operations'));
  readonly isCompliance     = computed(() => this.auth.hasRole('Compliance'));
  readonly isAdmin          = computed(() => this.auth.hasRole('Admin'));

  // ── Submissions ───────────────────────────────────────────────────────────

  /** Only Agent can create new submissions; all others (incl. Admin) are view-only. */
  readonly canCreateSubmission = computed(() => this.auth.hasRole('Agent'));

  // ── Rules & Scoring ───────────────────────────────────────────────────────

  /**
   * Operations can create/edit/delete rules.
   * Underwriter, UWAssistant, and Admin see rules but cannot mutate them.
   */
  readonly canEditRules = computed(() => this.auth.hasRole('Operations'));
  readonly isRulesViewOnly = computed(() =>
    this.auth.hasRole('Underwriter', 'UWAssistant', 'Admin')
  );

  // ── Party / Distribution ─────────────────────────────────────────────────

  /** Agent and Admin can see agents list. Agent sees only their own party. */
  readonly canViewParty = computed(() => this.auth.hasRole('Agent', 'Admin'));
}
