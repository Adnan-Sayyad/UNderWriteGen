import { Component, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';

interface NavItem  { label: string; icon: string; route: string; roles: string[]; }
interface NavGroup { title: string; items: NavItem[]; }

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  readonly user = computed(() => this.auth.currentUser());

  /**
   * Navigation structure.
   * roles: [] means the item is visible to every authenticated user.
   *
   * Role matrix:
   *  Agent          – Submissions (own), Agents (view), Customer Parties (own)
   *  Underwriter    – UW Workbench, Risk & Evidence, Rules & Scoring (view)
   *  UWAssistant    – Risk & Evidence, Rules & Scoring (view), Compliance > Checklists
   *  PricingAnalyst – Create Quote, Quotes, Parameters
   *  Operations     – Rules & Scoring (edit), Policy Desk
   *  Compliance     – Checklists, Exception Log, Reports
   *  Admin          – Everything (Submissions view-only, Rules view-only)
   */
  readonly navGroups: NavGroup[] = [
    // ── Submissions ─────────────────────────────────────────────────────────
    {
      title: 'Submissions',
      items: [
        { label: 'Submissions', icon: 'bi-file-earmark-text', route: '/submissions', roles: [] },
      ],
    },

    // ── Distribution & Party  (Agent + Admin) ────────────────────────────
    {
      title: 'Distribution & Party',
      items: [
        { label: 'Agents',           icon: 'bi-person-badge', route: '/party/agents',  roles: ['Agent', 'Admin'] },
        { label: 'Customer Parties', icon: 'bi-people',       route: '/party/parties', roles: ['Agent', 'Admin'] },
      ],
    },

    // ── Underwriting ────────────────────────────────────────────────────────
    {
      title: 'Underwriting',
      items: [
        { label: 'UW Workbench', icon: 'bi-briefcase', route: '/underwriting', roles: ['Underwriter', 'Admin'] },
      ],
    },

    // ── Rules & Scoring  (Underwriter + UWAssistant view; Operations edit) ─
    {
      title: 'Rules & Scoring',
      items: [
        { label: 'UW Rules',     icon: 'bi-diagram-3',    route: '/rules/list',        roles: ['Underwriter', 'UWAssistant', 'Operations', 'Admin'] },
        { label: 'Risk Scoring', icon: 'bi-speedometer2', route: '/rules/risk-scores', roles: ['Underwriter', 'UWAssistant', 'Operations', 'Admin'] },
      ],
    },

    // ── Pricing & Quotation  (PricingAnalyst + Admin) ───────────────────────
    {
      title: 'Pricing & Quotation',
      items: [
        { label: 'Create Quote', icon: 'bi-plus-circle', route: '/pricing/quotes/new', roles: ['PricingAnalyst', 'Admin'] },
        { label: 'Quotes',       icon: 'bi-calculator',  route: '/pricing/quotes',     roles: ['PricingAnalyst', 'Admin'] },
        { label: 'Parameters',   icon: 'bi-sliders',     route: '/pricing/params',     roles: ['PricingAnalyst', 'Admin'] },
      ],
    },

    // ── Policy  (Operations + Admin) ────────────────────────────────────────
    {
      title: 'Policy',
      items: [
        { label: 'Policy Desk', icon: 'bi-journal-bookmark', route: '/policy', roles: ['Operations', 'Admin'] },
      ],
    },

    // ── Compliance  (Compliance + UWAssistant for checklists + Admin) ───────
    {
      title: 'Compliance',
      items: [
        { label: 'Checklists',    icon: 'bi-clipboard2-check', route: '/compliance/checklists',  roles: ['Compliance', 'UWAssistant', 'Admin'] },
        { label: 'Exception Log', icon: 'bi-bug',              route: '/compliance/exceptions', roles: ['Compliance', 'Admin'] },
      ],
    },

    // ── Settings  (Admin only) ───────────────────────────────────────────────
    {
      title: 'Settings',
      items: [
        { label: 'Admin Console', icon: 'bi-gear',         route: '/admin',            roles: ['Admin'] },
        { label: 'Audit Logs',    icon: 'bi-journal-text', route: '/admin/audit-logs', roles: ['Admin'] },
      ],
    },
  ];

  constructor(readonly auth: AuthService) {}

  visibleGroups() {
    return this.navGroups
      .map(g => ({
        ...g,
        items: g.items.filter(i => !i.roles.length || this.auth.hasRole(...i.roles)),
      }))
      .filter(g => g.items.length);
  }
}
