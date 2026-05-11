import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import {
  Referral, ReferralStatus, Authority,
  AUTHORITIES, REFERRAL_STATUSES,
} from '../../models/rules.model';

@Component({
  selector: 'app-referral-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './referral-list.html',
  styleUrl: './referral-list.css',
})
export class ReferralListPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly referrals    = signal<Referral[]>([]);
  readonly loading      = signal(false);
  readonly updating     = signal<string | null>(null);
  readonly filterStatus = signal<ReferralStatus | ''>('');
  readonly filterAuth   = signal<Authority | ''>('');
  readonly filterSub    = signal('');
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly createOpen   = signal(false);
  readonly saving       = signal(false);

  readonly statuses    = REFERRAL_STATUSES;
  readonly authorities = AUTHORITIES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Rules', route: '/rules/list' },
    { label: 'Referrals' },
  ];

  readonly filtered = computed(() => {
    const st = this.filterStatus();
    const au = this.filterAuth();
    const sub = this.filterSub().toLowerCase();
    return this.referrals().filter(r =>
      (!st || r.status === st) &&
      (!au || r.requiredAuthority === au) &&
      (!sub || r.submissionID.toLowerCase().includes(sub)));
  });

  readonly createForm = this.fb.group({
    submissionID:      ['', Validators.required],
    raisedBy:          ['', Validators.required],
    reason:            ['', [Validators.required, Validators.minLength(5)]],
    requiredAuthority: ['UW1' as Authority, Validators.required],
    assignedTo:        ['', Validators.required],
  });

  constructor(private svc: RulesApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getReferrals().subscribe({
      next: r => { this.referrals.set(r); this.loading.set(false); },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load referrals.'); },
    });
  }

  updateStatus(r: Referral, status: ReferralStatus): void {
    this.updating.set(r.referralID);
    this.svc.updateReferralStatus(r.referralID, status).subscribe({
      next: updated => {
        this.updating.set(null);
        this.referrals.update(list => list.map(x => x.referralID === r.referralID ? updated : x));
        this.flash('success', `Referral ${status.toLowerCase()}.`);
      },
      error: () => { this.updating.set(null); this.flash('danger', 'Failed to update referral.'); },
    });
  }

  openCreate(): void {
    this.createForm.reset({ submissionID: '', raisedBy: '', reason: '', requiredAuthority: 'UW1', assignedTo: '' });
    this.createOpen.set(true);
  }

  closeCreate(): void { this.createOpen.set(false); }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    const v = this.createForm.value;
    this.saving.set(true);
    this.svc.createReferral({
      submissionID:      v.submissionID!,
      raisedBy:          v.raisedBy!,
      reason:            v.reason!,
      requiredAuthority: v.requiredAuthority as Authority,
      assignedTo:        v.assignedTo!,
    }).subscribe({
      next: () => { this.saving.set(false); this.closeCreate(); this.load(); this.flash('success', 'Referral raised.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Failed to raise referral.'); },
    });
  }

  statusClass(s: ReferralStatus): string {
    const map: Record<ReferralStatus, string> = {
      Pending: 'bg-warning text-dark', Approved: 'bg-success', Rejected: 'bg-danger',
    };
    return map[s] ?? 'bg-secondary';
  }

  authorityClass(a: Authority): string {
    const map: Record<Authority, string> = {
      UW1: 'bg-primary', UW2: 'bg-info text-dark',
      UWManager: 'bg-warning text-dark', Committee: 'bg-danger',
    };
    return map[a] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
