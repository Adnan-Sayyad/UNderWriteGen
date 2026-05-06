import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PolicyApiService } from '../../services/policy-api.service';
import { Policy, PolicyStatus } from '../../models/policy.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

const STATUSES: PolicyStatus[] = ['Active', 'Cancelled', 'Expired'];

@Component({
  selector: 'app-policy-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.css',
})
export class PolicyListPage implements OnInit {
  readonly policies     = signal<Policy[]>([]);
  readonly loading      = signal(false);
  readonly filterStatus = signal('');
  readonly searchQuery  = signal('');
  readonly statuses     = STATUSES;

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    return this.policies().filter(p =>
      (!q || p.policyNumber?.toLowerCase().includes(q)) && (!s || p.status === s)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policy Desk' },
  ];

  constructor(private svc: PolicyApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.policies.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : s === 'Cancelled' ? 'bg-danger' : 'bg-secondary';
  }
}
