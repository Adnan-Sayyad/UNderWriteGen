import { Component, OnInit, OnDestroy, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
import { ReportsNav } from '../../components/reports-nav/reports-nav';
import {
  ReportSummaryDto,
  HitRatioDto,
  TatDto,
  ReferralRateDto,
  PremiumDistributionDto,
  RiskMixDto,
  UWProductivityDto,
  GenerateReportRequest,
  ReportScope,
} from '../../models/reports.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, ReportsNav],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardPage implements OnInit, OnDestroy {

  // ── Data signals ───────────────────────────────────────────────────────────
  readonly reports      = signal<ReportSummaryDto[]>([]);
  readonly hitRatio     = signal<HitRatioDto[]>([]);
  readonly tat          = signal<TatDto[]>([]);
  readonly referralRate = signal<ReferralRateDto[]>([]);
  readonly premiumDist  = signal<PremiumDistributionDto[]>([]);
  readonly riskMix      = signal<RiskMixDto[]>([]);
  readonly uwProd       = signal<UWProductivityDto[]>([]);

  // ── UI state ───────────────────────────────────────────────────────────────
  readonly loading      = signal(false);
  readonly showModal    = signal(false);
  readonly generating   = signal(false);
  readonly collecting   = signal(false);
  readonly alertMsg     = signal<{ type: string; text: string } | null>(null);

  // Tracks which specific endpoints failed (vs genuinely empty)
  readonly failedEndpoints = signal<string[]>([]);

  // Pagination
  readonly currentPage  = signal(1);
  readonly pageSize     = 20;
  readonly hasMorePages = signal(false);

  // ── Filter fields ──────────────────────────────────────────────────────────
  filterScope = '';
  filterFrom  = `${new Date().getFullYear()}-01-01`;
  filterTo    = `${new Date().getFullYear()}-12-31`;

  // ── Generate form fields ───────────────────────────────────────────────────
  genScope      = '';
  genScopeValue = '';
  genFrom       = '';
  genTo         = '';

  readonly scopes: ReportScope[] = ['Product', 'Region', 'Agent', 'Period'];

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Reports & Analytics' },
  ];

  constructor(private svc: ReportsApiService) {}

  ngOnInit() { this.loadAll(); }

  ngOnDestroy() {
    // Ensure body scroll is restored if modal was open when navigating away
    document.body.style.overflow = '';
  }

  // ESC key closes modal
  @HostListener('document:keydown.escape')
  onEsc() { if (this.showModal()) this.closeModal(); }

  // ── Safe wrapper: catches errors and records which endpoint failed ─────────
  private safe<T>(obs: Observable<T[]>, label: string): Observable<T[]> {
    return obs.pipe(
      catchError(() => {
        this.failedEndpoints.update(prev => [...prev, label]);
        return of([] as T[]);
      })
    );
  }

  // ── Load everything in parallel ────────────────────────────────────────────
  loadAll(): void {
    this.loading.set(true);
    this.failedEndpoints.set([]);
    const s = this.filterScope || undefined;
    const f = this.filterFrom  || undefined;
    const t = this.filterTo    || undefined;

    forkJoin({
      reports:  this.safe(this.svc.getReports(this.currentPage(), this.pageSize), 'reports'),
      hitRatio: this.safe(this.svc.getHitRatio(s, f, t),              'hit-ratio'),
      tat:      this.safe(this.svc.getTat(s, f, t),                   'tat'),
      referral: this.safe(this.svc.getReferralRate(s, f, t),          'referral-rate'),
      premium:  this.safe(this.svc.getPremiumDistribution(s, f, t),   'premium'),
      riskMix:  this.safe(this.svc.getRiskMix(s, f, t),              'risk-mix'),
      uwProd:   this.safe(this.svc.getUWProductivity(s, f, t),       'uw-productivity'),
    }).subscribe({
      next: res => {
        const rpts = this.toArr<ReportSummaryDto>(res.reports);
        this.reports.set(rpts);
        this.hasMorePages.set(rpts.length === this.pageSize);
        this.hitRatio.set(this.toArr(res.hitRatio));
        this.tat.set(this.toArr(res.tat));
        this.referralRate.set(this.toArr(res.referral));
        this.premiumDist.set(this.toArr(res.premium));
        this.riskMix.set(this.toArr(res.riskMix));
        this.uwProd.set(this.toArr(res.uwProd));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.flash('danger', 'Failed to load analytics. Check that the Reporting service is running on port 8090.');
      },
    });
  }

  // ── Collect Now — pulls fresh data from all microservices immediately ────────
  collectNow(): void {
    this.collecting.set(true);
    this.svc.triggerCollect().subscribe({
      next: res => {
        this.collecting.set(false);
        const failed = res.results.filter(r => r.status !== 'ok').map(r => r.productLine);
        if (failed.length) {
          this.flash('warning', `Collection complete. Some lines had errors: ${failed.join(', ')}`);
        } else {
          this.flash('success', 'Data refreshed from all services — charts updated.');
        }
        this.loadAll();   // reload charts immediately after collection
      },
      error: () => {
        this.collecting.set(false);
        this.flash('danger', 'Collection failed. Is the Reporting service running on port 8090?');
      },
    });
  }

  // ── Pagination ─────────────────────────────────────────────────────────────
  applyFilters(): void {
    this.currentPage.set(1);
    this.loadAll();
  }

  prevPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
      this.loadAll();
    }
  }

  nextPage(): void {
    if (this.hasMorePages()) {
      this.currentPage.update(p => p + 1);
      this.loadAll();
    }
  }

  // ── KPI computed values ────────────────────────────────────────────────────
  get totalReports()   { return this.reports().length; }
  get totalQuotes()    { return this.hitRatio().reduce((s, r) => s + r.quotes, 0); }
  get totalBound()     { return this.hitRatio().reduce((s, r) => s + r.boundPolicies, 0); }

  get avgHitRatio(): number {
    const xs = this.hitRatio();
    return xs.length ? xs.reduce((s, r) => s + r.hitRatioPercent, 0) / xs.length : 0;
  }
  get avgTatHours(): number {
    const xs = this.tat();
    return xs.length ? xs.reduce((s, r) => s + r.avgHours, 0) / xs.length : 0;
  }
  get avgReferralRate(): number {
    const xs = this.referralRate();
    return xs.length ? xs.reduce((s, r) => s + r.referralRatePercent, 0) / xs.length : 0;
  }
  get avgPremium(): number {
    const xs = this.premiumDist();
    return xs.length ? xs.reduce((s, r) => s + r.avgPremium, 0) / xs.length : 0;
  }
  get totalGrossPremium(): number {
    return this.premiumDist().reduce((s, r) => s + r.totalGrossPremium, 0);
  }

  // ── Chart bar helpers ──────────────────────────────────────────────────────
  barPct(value: number, max: number): number {
    return max > 0 ? Math.min(Math.round((value / max) * 100), 100) : 0;
  }
  get maxHitRatio()   { return Math.max(...this.hitRatio().map(r => r.hitRatioPercent), 0.01); }
  get maxAvgHours()   { return Math.max(...this.tat().map(r => r.avgHours), 0.01); }
  get maxReferral()   { return Math.max(...this.referralRate().map(r => r.referralRatePercent), 0.01); }
  get maxAvgPremium() { return Math.max(...this.premiumDist().map(r => r.avgPremium), 0.01); }
  get maxDecisions()  { return Math.max(...this.uwProd().map(r => r.totalDecisions), 0.01); }

  riskPct(count: number, item: RiskMixDto): number {
    return item.total > 0 ? Math.round((count / item.total) * 100) : 0;
  }

  // ── Generate modal ─────────────────────────────────────────────────────────
  openModal(): void {
    this.showModal.set(true);
    document.body.style.overflow = 'hidden';   // prevent background scroll
  }

  closeModal(): void {
    this.showModal.set(false);
    document.body.style.overflow = '';
    this.resetGenForm();
  }

  generateReport(): void {
    if (!this.genScope || !this.genScopeValue || !this.genFrom || !this.genTo) {
      this.flash('danger', 'All fields are required to generate a report.');
      return;
    }
    if (this.genFrom >= this.genTo) {
      this.flash('danger', 'Period Start must be before Period End.');
      return;
    }
    this.generating.set(true);
    const req: GenerateReportRequest = {
      scope:       this.genScope,
      scopeValue:  this.genScopeValue,
      periodStart: this.genFrom,
      periodEnd:   this.genTo,
    };
    this.svc.generateReport(req).subscribe({
      next: () => {
        this.generating.set(false);
        this.closeModal();
        this.flash('success', 'Report generated successfully.');
        this.currentPage.set(1);
        this.loadAll();
      },
      error: () => {
        this.generating.set(false);
        this.flash('danger', 'Failed to generate report. Verify that snapshot data exists for the selected period.');
      },
    });
  }

  // ── Date formatting (safe — returns — on invalid input) ────────────────────
  formatDate(d: string | null): string {
    if (!d) return '—';
    try {
      const dt = new Date(d);
      return isNaN(dt.getTime()) ? '—' : dt.toLocaleDateString('en-GB');
    } catch {
      return '—';
    }
  }

  private resetGenForm(): void {
    this.genScope = '';  this.genScopeValue = '';
    this.genFrom  = '';  this.genTo         = '';
  }

  private toArr<T>(val: T[] | unknown): T[] {
    if (Array.isArray(val)) return val;
    if (val && Array.isArray((val as { data?: unknown }).data)) {
      return (val as { data: T[] }).data;
    }
    return [];
  }

  private flash(type: string, text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
