import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PolicyApiService } from '../../services/policy-api.service';
import { Renewal, RenewalStatus } from '../../models/policy.model';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-renewal-list',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, EmptyState],
  templateUrl: './renewal-list.html',
  styleUrl: './renewal-list.css',
})
export class RenewalListPage implements OnInit {
  readonly renewals      = signal<Renewal[]>([]);
  readonly loading       = signal(true);
  readonly filterStatus  = signal<RenewalStatus | ''>('');
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly currentPage   = signal(0);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policies', route: '/policy' },
    { label: 'Renewals' },
  ];

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return s ? this.renewals().filter(r => r.status === s) : this.renewals();
  });

  readonly pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i)
  );

  constructor(private svc: PolicyApiService) {}

  ngOnInit() { this.load(); }

  load(page = 0) {
    this.loading.set(true);
    const req: any = { page, size: PAGE_SIZE, sort: 'offeredDate', direction: 'desc' };
    this.svc.getRenewals(req).subscribe({
      next: (res: any) => {
        this.renewals.set(res?.content ?? res?.data ?? []);
        this.totalPages.set(res?.totalPages ?? 1);
        this.totalElements.set(res?.totalElements ?? 0);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goToPage(page: number) {
    if (page < 0 || page >= this.totalPages()) return;
    this.load(page);
  }

  updateStatus(r: Renewal, status: RenewalStatus) {
    this.svc.updateRenewal(r.renewalId, { status }).subscribe({
      next: (res: any) => {
        const updated: Renewal = res?.data ?? res;
        this.renewals.update(list => list.map(x => x.renewalId === r.renewalId ? { ...x, status: updated.status } : x));
        this.flash('success', `Renewal marked ${status}.`);
      },
      error: () => this.flash('danger', 'Failed to update renewal.'),
    });
  }

  statusClass(status: RenewalStatus): string {
    const map: Record<RenewalStatus, string> = {
      Offered: 'bg-info text-dark', Accepted: 'bg-success', Declined: 'bg-danger',
    };
    return map[status] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
