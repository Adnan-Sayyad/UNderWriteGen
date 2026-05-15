import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
import { PremiumDistributionDto, ReportScope, ReportSummaryDto } from '../../models/reports.model';
import { PremiumDistributionChart } from '../../components/premium-distribution/premium-distribution';
import { ReportsNav } from '../../components/reports-nav/reports-nav';

@Component({
  selector: 'app-premium-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, PremiumDistributionChart, ReportsNav],
  templateUrl: './premium-page.html',
  styleUrl: './premium-page.css',
})
export class PremiumPage implements OnInit {
  readonly data      = signal<PremiumDistributionDto[]>([]);
  readonly snapshots = signal<ReportSummaryDto[]>([]);
  readonly loading   = signal(false);
  readonly hasError  = signal(false);

  filterScope = '';
  filterFrom  = `${new Date().getFullYear()}-01-01`;
  filterTo    = `${new Date().getFullYear()}-12-31`;

  readonly scopes: ReportScope[] = ['Product', 'Region', 'Agent', 'Period'];

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Reports', route: '/reports' },
    { label: 'Premium Distribution' },
  ];

  constructor(private svc: ReportsApiService) {}

  ngOnInit() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.hasError.set(false);
    const s = this.filterScope || undefined;
    const f = this.filterFrom  || undefined;
    const t = this.filterTo    || undefined;

    this.svc.getPremiumDistribution(s, f, t).pipe(
      catchError(() => { this.hasError.set(true); return of([] as PremiumDistributionDto[]); })
    ).subscribe({
      next: res => { this.data.set(Array.isArray(res) ? res : []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });

    this.svc.getReports(1, 100).pipe(
      catchError(() => of([] as ReportSummaryDto[]))
    ).subscribe(res => {
      let rows = Array.isArray(res) ? res : [];
      if (f) rows = rows.filter(r => r.generatedDate >= f);
      if (t) rows = rows.filter(r => r.generatedDate <= t + 'T23:59:59');
      if (s) rows = rows.filter(r => r.scope === s);
      rows = rows.filter(r => r.avgPremium > 0);
      this.snapshots.set(rows.sort((a, b) => b.generatedDate.localeCompare(a.generatedDate)));
    });
  }

  get totalGross()    { return this.data().reduce((s, d) => s + d.totalGrossPremium, 0); }
  get totalPolicies() { return this.data().reduce((s, d) => s + d.policyCount, 0); }
  get overallAvg()    {
    const xs = this.data().filter(d => d.avgPremium > 0);
    return xs.length ? xs.reduce((s, d) => s + d.avgPremium, 0) / xs.length : 0;
  }
  topEarner(): PremiumDistributionDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.totalGrossPremium > b.totalGrossPremium ? a : b)
      : null;
  }
  highestAvg(): PremiumDistributionDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.avgPremium > b.avgPremium ? a : b)
      : null;
  }
}
