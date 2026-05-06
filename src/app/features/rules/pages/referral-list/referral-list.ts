import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import { Referral, ReferralStatus } from '../../models/rules.model';

const DEFAULT_PAGE = { page: 0, size: 20, sort: 'createdDate', direction: 'desc' as const };

@Component({
  selector: 'app-referral-list',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, EmptyState],
  templateUrl: './referral-list.html',
  styleUrl: './referral-list.css',
})
export class ReferralListPage implements OnInit {
  readonly referrals    = signal<Referral[]>([]);
  readonly loading      = signal(true);
  readonly filterStatus = signal<ReferralStatus | ''>('');
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Rules', route: '/rules/list' },
    { label: 'Referrals' },
  ];

  readonly statuses: (ReferralStatus | '')[] = ['', 'Pending', 'Approved', 'Rejected'];

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return s ? this.referrals().filter(r => r.status === s) : this.referrals();
  });

  constructor(private svc: RulesApiService) {}

  ngOnInit() {
    this.svc.getReferrals(DEFAULT_PAGE).subscribe({
      next: (res: any) => {
        this.referrals.set(res?.content ?? res?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  updateStatus(r: Referral, status: ReferralStatus) {
    this.svc.updateReferral(r.referralId, { status }).subscribe({
      next: (res: any) => {
        const updated: Referral = res?.data ?? res;
        this.referrals.update(list => list.map(x => x.referralId === r.referralId ? { ...x, status: updated.status } : x));
        this.flash('success', `Referral ${status.toLowerCase()}.`);
      },
      error: () => this.flash('danger', 'Failed to update referral.'),
    });
  }

  statusClass(status: ReferralStatus): string {
    const map: Record<ReferralStatus, string> = {
      Pending: 'bg-warning text-dark', Approved: 'bg-success', Rejected: 'bg-danger',
    };
    return map[status] ?? 'bg-secondary';
  }

  authorityClass(a: string): string {
    const map: Record<string, string> = {
      UW1: 'bg-primary', UW2: 'bg-info text-dark', UWManager: 'bg-warning text-dark', Committee: 'bg-danger',
    };
    return map[a] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
