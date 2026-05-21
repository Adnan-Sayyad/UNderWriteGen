import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PolicyApiService } from '../../services/policy-api.service';
import { Policy } from '../../models/policy.model';

type Tab = 'overview' | 'coverage';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, DatePipe],
  templateUrl: './policy-detail.html',
  styleUrl: './policy-detail.css',
})
export class PolicyDetailPage implements OnInit {
  readonly policyId  = signal<string>('');
  readonly policy    = signal<Policy | null>(null);
  readonly loading   = signal(true);
  readonly alertMsg  = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly activeTab = signal<Tab>('overview');

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policies', route: '/policy' },
    { label: 'Policy Detail' },
  ];

  constructor(private svc: PolicyApiService, private route: ActivatedRoute) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.policyId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.svc.getById(id).subscribe({
      next: (res: any) => { this.policy.set(res?.data ?? res); this.loading.set(false); },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load policy.'); },
    });
  }

  selectTab(tab: Tab) { this.activeTab.set(tab); }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Active: 'bg-success', Cancelled: 'bg-danger', Expired: 'bg-secondary',
    };
    return map[status] ?? 'bg-secondary';
  }

  coverageEntries(): { key: string; value: string }[] {
    const cov = this.policy()?.coverageJSON ?? {};
    return Object.entries(cov).map(([key, value]) => ({ key, value: String(value) }));
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
