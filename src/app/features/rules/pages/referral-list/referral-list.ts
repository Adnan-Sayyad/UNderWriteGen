import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pager } from '../../components/pager/pager';
import { RulesApiService } from '../../services/rules-api.service';
import {
  Referral, ReferralStatus, Authority,
  AUTHORITIES, REFERRAL_STATUSES, DEFAULT_PAGE_SIZE,
} from '../../models/rules.model';

@Component({
  selector: 'app-referral-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState, Pager],
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

  // Pagination
  readonly page          = signal(0);
  readonly size          = signal(DEFAULT_PAGE_SIZE);
  readonly totalElements = signal(0);
  readonly totalPages    = signal(0);

  readonly createOpen   = signal(false);
  readonly saving       = signal(false);

  readonly statuses    = REFERRAL_STATUSES;
  readonly authorities = AUTHORITIES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Rules', route: '/rules/list' },
    { label: 'Referrals' },
  ];

  readonly createForm = this.fb.group({
    submissionID:      ['', Validators.required],
    raisedBy:          ['', Validators.required],
    reason:            ['', [Validators.required, Validators.minLength(5)]],
    requiredAuthority: ['UW1' as Authority, Validators.required],
    assignedTo:        ['', Validators.required],
  });

  private searchTimer: any;

  constructor(private svc: RulesApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    const sub = this.filterSub().trim();
    this.svc.getReferrals(this.page(), this.size(), {
      status:       this.filterStatus(),
      authority:    this.filterAuth(),
      // Only send if it looks like a Guid — backend expects [FromQuery] Guid?
      submissionId: this.isGuid(sub) ? sub : undefined,
    }).subscribe({
      next: r => {
        this.referrals.set(r.content);
        this.totalElements.set(r.totalElements);
        this.totalPages.set(r.totalPages);
        this.page.set(r.page);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load referrals.'); },
    });
  }

  setStatus(v: string) { this.filterStatus.set(v as ReferralStatus | ''); this.page.set(0); this.load(); }
  setAuth(v: string)   { this.filterAuth.set(v as Authority | ''); this.page.set(0); this.load(); }

  /** Debounced submission-id search box. */
  onSubInput(v: string): void {
    this.filterSub.set(v);
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => { this.page.set(0); this.load(); }, 300);
  }

  onPageChange(p: number) { this.page.set(p); this.load(); }
  onSizeChange(s: number) { this.size.set(s); this.page.set(0); this.load(); }

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
      submissionID:      (v.submissionID ?? '').trim(),
      raisedBy:          (v.raisedBy ?? '').trim(),
      reason:            (v.reason ?? '').trim(),
      requiredAuthority: v.requiredAuthority as Authority,
      assignedTo:        (v.assignedTo ?? '').trim(),
    }).subscribe({
      next: () => { this.saving.set(false); this.closeCreate(); this.page.set(0); this.load(); this.flash('success', 'Referral raised.'); },
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

  private isGuid(s: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(s);
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
