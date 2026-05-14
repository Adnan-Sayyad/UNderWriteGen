import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
import { HitRatioDto, ReportScope, ReportSummaryDto } from '../../models/reports.model';
import { HitRatioChart } from '../../components/hit-ratio-chart/hit-ratio-chart';
import { ReportsNav } from '../../components/reports-nav/reports-nav';

@Component({
  selector: 'app-hit-ratio-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, HitRatioChart, ReportsNav],
  templateUrl: './hit-ratio-page.html',
  styleUrl: './hit-ratio-page.css',
})
export class HitRatioPage implements OnInit {
  readonly data      = signal<HitRatioDto[]>([]);
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
    { label: 'Hit Ratio Analysis' },
  ];

  constructor(private svc: ReportsApiService) {}

  ngOnInit() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.hasError.set(false);
    const s = this.filterScope || undefined;
    const f = this.filterFrom  || undefined;
    const t = this.filterTo    || undefined;

    this.svc.getHitRatio(s, f, t).pipe(
      catchError(() => { this.hasError.set(true); return of([] as HitRatioDto[]); })
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
      rows = rows.filter(r => r.hitRatio > 0 || r.avgPremium > 0 || r.referralRate > 0);
      this.snapshots.set(rows.sort((a, b) => b.generatedDate.localeCompare(a.generatedDate)));
    });
  }

  get totalQuotes()  { return this.data().reduce((s, d) => s + d.quotes, 0); }
  get totalBound()   { return this.data().reduce((s, d) => s + d.boundPolicies, 0); }
  get overallRate()  {
    return this.totalQuotes > 0
      ? Math.round((this.totalBound / this.totalQuotes) * 100 * 10) / 10
      : 0;
  }
  bestScope(): HitRatioDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.hitRatioPercent > b.hitRatioPercent ? a : b)
      : null;
  }
  worstScope(): HitRatioDto | null {
    return this.data().length
      ? this.data().reduce((a, b) => a.hitRatioPercent < b.hitRatioPercent ? a : b)
      : null;
  }
}
