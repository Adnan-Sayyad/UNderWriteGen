import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
import { ReferralRateDto, ReportScope, ReportSummaryDto } from '../../models/reports.model';
import { ReferralPatternsChart } from '../../components/referral-patterns/referral-patterns';
import { ReportsNav } from '../../components/reports-nav/reports-nav';

@Component({
  selector: 'app-referrals-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, ReferralPatternsChart, ReportsNav],
  templateUrl: './referrals-page.html',
  styleUrl: './referrals-page.css',
})
export class ReferralsPage implements OnInit {
  readonly data      = signal<ReferralRateDto[]>([]);
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
    { label: 'Referral Patterns' },
  ];

  constructor(private svc: ReportsApiService) {}

  ngOnInit() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.hasError.set(false);
    const s = this.filterScope || undefined;
    const f = this.filterFrom  || undefined;
    const t = this.filterTo    || undefined;

    this.svc.getReferralRate(s, f, t).pipe(
      catchError(() => { this.hasError.set(true); return of([] as ReferralRateDto[]); })
    ).subscribe({
      next: res => { this.data.set(Array.isArray(res) ? res : []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });

    // Individual dated snapshots for the breakdown table
    this.svc.getReports(1, 100).pipe(
      catchError(() => of([] as ReportSummaryDto[]))
    ).subscribe(res => {
      let rows = Array.isArray(res) ? res : [];
      if (f) rows = rows.filter(r => r.generatedDate >= f);
      if (t) rows = rows.filter(r => r.generatedDate <= t + 'T23:59:59');
      if (s) rows = rows.filter(r => r.scope === s);
      rows = rows.filter(r => r.referralRate > 0);
      this.snapshots.set(rows.sort((a, b) => b.generatedDate.localeCompare(a.generatedDate)));
    });
  }

  get totalReferrals() { return this.data().reduce((s, d) => s + d.totalReferrals, 0); }
  get totalQuotes()    { return this.data().reduce((s, d) => s + d.quotes, 0); }
  get overallRate()    {
    return this.totalQuotes > 0
      ? Math.round((this.totalReferrals / this.totalQuotes) * 100 * 10) / 10
      : 0;
  }
  highestReferral(): ReferralRateDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.referralRatePercent > b.referralRatePercent ? a : b)
      : null;
  }
  lowestReferral(): ReferralRateDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.referralRatePercent < b.referralRatePercent ? a : b)
      : null;
  }
  riskLevel(rate: number): string {
    if (rate >= 40) return 'danger';
    if (rate >= 20) return 'warning';
    return 'success';
  }
  riskLabel(rate: number): string {
    if (rate >= 40) return 'High';
    if (rate >= 20) return 'Medium';
    return 'Low';
  }
}
