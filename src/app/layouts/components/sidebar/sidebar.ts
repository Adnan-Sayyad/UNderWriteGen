import { Component, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth/auth.service';

interface NavItem { label: string; icon: string; route: string; roles: string[]; }
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

  readonly navGroups: NavGroup[] = [
    {
      title: 'Submissions',
      items: [
        { label: 'Submissions',     icon: 'bi-file-earmark-text', route: '/submissions',   roles: [] },
        { label: 'Parties',         icon: 'bi-people',            route: '/party',          roles: ['Agent','Underwriter','Admin','Operations'] },
      ],
    },
    {
      title: 'Underwriting',
      items: [
        { label: 'UW Workbench',    icon: 'bi-briefcase',         route: '/underwriting',  roles: ['Underwriter','UWAssistant'] },
        { label: 'Risk & Evidence', icon: 'bi-shield-check',      route: '/risk',           roles: ['Underwriter','UWAssistant'] },
        { label: 'Rules & Scoring', icon: 'bi-diagram-3',         route: '/rules',          roles: ['Underwriter','Admin'] },
      ],
    },
    {
      title: 'Pricing & Policy',
      items: [
        { label: 'Pricing Console', icon: 'bi-calculator',        route: '/pricing',        roles: ['PricingAnalyst','Underwriter','Admin'] },
        { label: 'Policy Desk',     icon: 'bi-journal-bookmark',  route: '/policy',         roles: ['Operations','Underwriter','Admin'] },
      ],
    },
    {
      title: 'Compliance',
      items: [
        { label: 'Checklists',        icon: 'bi-clipboard2-check',   route: '/compliance/checklists',         roles: ['Compliance','Admin'] },
        { label: 'Authority Breaches',icon: 'bi-shield-exclamation', route: '/compliance/authority-breaches', roles: ['Compliance','Admin'] },
        { label: 'Exception Log',     icon: 'bi-bug',                route: '/compliance/exceptions',         roles: ['Compliance','Admin'] },
        { label: 'Audit Log',         icon: 'bi-journal-text',       route: '/compliance/audit-log',          roles: ['Compliance','Admin'] },
        { label: 'Reports',           icon: 'bi-bar-chart-line',     route: '/reports',                       roles: ['Compliance','PricingAnalyst','Admin','Underwriter'] },
      ],
    },
    {
      title: 'Settings',
      items: [
        { label: 'Admin Console',   icon: 'bi-gear',              route: '/admin',           roles: ['Admin'] },
        { label: 'Audit Logs',      icon: 'bi-journal-text',      route: '/admin/audit-logs',roles: ['Admin'] },
      ],
    },
  ];

  constructor(readonly auth: AuthService) {}

  visibleGroups() {
    return this.navGroups.map(g => ({
      ...g,
      items: g.items.filter(i => !i.roles.length || this.auth.hasRole(...i.roles)),
    })).filter(g => g.items.length);
  }
}
