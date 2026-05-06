import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
import { UWReport, ReportFilter, ReportScope } from '../../models/reports.model';
import { environment } from '../../../../../environments/environment';

const SCOPES: ReportScope[] = ['Product', 'Region', 'Agent', 'Period'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardPage implements OnInit {
  readonly reports  = signal<UWReport[]>([]);
  readonly loading  = signal(false);
  readonly scopes   = SCOPES;
  readonly backendUrl = `${environment.apiBaseUrl}/reports`;

  filterScope: ReportScope | '' = '';
  filterFrom  = '2025-01-01';
  filterTo    = '2025-12-31';

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Reports & Analytics' },
  ];

  constructor(private svc: ReportsApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    const filter: ReportFilter = {};
    if (this.filterScope) filter.scope = this.filterScope as ReportScope;
    if (this.filterFrom)  filter.fromDate = this.filterFrom;
    if (this.filterTo)    filter.toDate   = this.filterTo;
    this.svc.getReports({ page: 0, size: 20 }, filter).subscribe({
      next: res => {
        const d: any = res;
        this.reports.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  get totalQuotes(): number {
    return this.reports().reduce((s, r) => s + (r.metrics?.quotes ?? 0), 0);
  }
  get avgHitRatio(): number {
    const rs = this.reports().filter(r => r.metrics?.hitRatio != null);
    return rs.length ? rs.reduce((s, r) => s + r.metrics.hitRatio, 0) / rs.length : 0;
  }
  get avgPremium(): number {
    const rs = this.reports().filter(r => r.metrics?.avgPremium != null);
    return rs.length ? rs.reduce((s, r) => s + r.metrics.avgPremium, 0) / rs.length : 0;
  }
  get avgTat(): number {
    const rs = this.reports().filter(r => r.metrics?.tat != null);
    return rs.length ? rs.reduce((s, r) => s + r.metrics.tat, 0) / rs.length : 0;
  }
  get avgReferralRate(): number {
    const rs = this.reports().filter(r => r.metrics?.referralRate != null);
    return rs.length ? rs.reduce((s, r) => s + r.metrics.referralRate, 0) / rs.length : 0;
  }

  formatDate(d: string): string {
    return d ? new Date(d).toLocaleDateString('en-GB').replace(/\//g, '-') : '';
  }
}
