import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PolicyApiService } from '../../services/policy-api.service';
import { Policy, Endorsement, Cancellation } from '../../models/policy.model';

type Tab = 'overview' | 'coverage' | 'endorsements' | 'cancellations';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, DatePipe],
  templateUrl: './policy-detail.html',
  styleUrl: './policy-detail.css',
})
export class PolicyDetailPage implements OnInit {
  readonly policyId             = signal<string>('');
  readonly policy               = signal<Policy | null>(null);
  readonly loading              = signal(true);
  readonly alertMsg             = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly activeTab            = signal<Tab>('overview');
  readonly endorsements         = signal<Endorsement[]>([]);
  readonly cancellations        = signal<Cancellation[]>([]);
  readonly loadingEndorsements  = signal(false);
  readonly loadingCancellations = signal(false);
  readonly approvingId          = signal<string | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policies', route: '/policy' },
    { label: 'Policy Detail' },
  ];

  constructor(
    private svc: PolicyApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.policyId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.svc.getById(id).subscribe({
      next: (res: any) => {
        this.policy.set(res?.data ?? res);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load policy.'); },
    });
  }

  selectTab(tab: Tab) {
    this.activeTab.set(tab);
    if (tab === 'endorsements' && !this.endorsements().length) this.loadEndorsements();
    if (tab === 'cancellations' && !this.cancellations().length) this.loadCancellations();
  }

  loadEndorsements() {
    this.loadingEndorsements.set(true);
    this.svc.getEndorsements(this.policyId()).subscribe({
      next: (res: any) => { this.endorsements.set(res?.content ?? res?.data ?? []); this.loadingEndorsements.set(false); },
      error: () => this.loadingEndorsements.set(false),
    });
  }

  loadCancellations() {
    this.loadingCancellations.set(true);
    this.svc.getCancellations(this.policyId()).subscribe({
      next: (res: any) => { this.cancellations.set(res?.content ?? res?.data ?? []); this.loadingCancellations.set(false); },
      error: () => this.loadingCancellations.set(false),
    });
  }

  approveEndorsement(id: string) {
    this.approvingId.set(id);
    this.svc.approveEndorsement(id).subscribe({
      next: () => { this.approvingId.set(null); this.endorsements.set([]); this.loadEndorsements(); this.flash('success', 'Endorsement approved.'); },
      error: () => { this.approvingId.set(null); this.flash('danger', 'Failed to approve endorsement.'); },
    });
  }

  approveCancellation(id: string) {
    this.approvingId.set(id);
    this.svc.approveCancellation(id).subscribe({
      next: () => { this.approvingId.set(null); this.cancellations.set([]); this.loadCancellations(); this.flash('success', 'Cancellation approved.'); },
      error: () => { this.approvingId.set(null); this.flash('danger', 'Failed to approve cancellation.'); },
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Active: 'bg-success', Cancelled: 'bg-danger', Expired: 'bg-secondary',
      Proposed: 'bg-warning text-dark', Approved: 'bg-success', Posted: 'bg-info text-dark',
      Requested: 'bg-warning text-dark',
    };
    return map[status] ?? 'bg-secondary';
  }

  coverageEntries(): { key: string; value: string }[] {
    const cov = this.policy()?.coverageJSON ?? {};
    return Object.entries(cov).map(([key, value]) => ({ key, value: String(value) }));
  }

  goEndorsement() { this.router.navigate(['/policy', this.policyId(), 'endorsement']); }
  goCancellation() { this.router.navigate(['/policy', this.policyId(), 'cancellation']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
