import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { catchError, of } from 'rxjs';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ReportsApiService } from '../../services/reports-api.service';
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
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardPage implements OnInit {
  readonly reports      = signal<ReportSummaryDto[]>([]);
  readonly hitRatio     = signal<HitRatioDto[]>([]);
  readonly tat          = signal<TatDto[]>([]);
  readonly referralRate = signal<ReferralRateDto[]>([]);
  readonly premiumDist  = signal<PremiumDistributionDto[]>([]);
  readonly riskMix      = signal<RiskMixDto[]>([]);
  readonly uwProd       = signal<UWProductivityDto[]>([]);
  readonly loading      = signal(false);
  readonly showModal    = signal(false);
  readonly generating   = signal(false);
  readonly alertMsg     = signal<{ type: string; text: string } | null>(null);

  filterScope = '';
  filterFrom  = '2025-01-01';
  filterTo    = '2025-12-31';

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

  loadAll(): void {
    this.loading.set(true);
    const s = this.filterScope || undefined;
    const f = this.filterFrom  || undefined;
    const t = this.filterTo    || undefined;

    forkJoin({
      reports:     this.svc.getReports(1, 50).pipe(catchError(() => of([]))),
      hitRatio:    this.svc.getHitRatio(s, f, t),
      tat:         this.svc.getTat(s, f, t),
      referral:    this.svc.getReferralRate(s, f, t),
      premium:     this.svc.getPremiumDistribution(s, f, t),
      riskMix:     this.svc.getRiskMix(s, f, t),
      uwProd:      this.svc.getUWProductivity(s, f, t),
    }).subscribe({
      next: res => {
        this.reports.set(this.toArr(res.reports));
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
        this.flash('danger', 'Failed to load analytics data. Check that the Reporting service is running on port 8090.');
      },
    });
  }

  // ── KPI computed values ────────────────────────────────────────────────────

  get totalReports()    { return this.reports().length; }

  get totalQuotes()     { return this.hitRatio().reduce((s, r) => s + r.quotes, 0); }

  get totalBound()      { return this.hitRatio().reduce((s, r) => s + r.boundPolicies, 0); }

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

  // ── Chart helpers ──────────────────────────────────────────────────────────

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

  openModal()  { this.showModal.set(true);  }
  closeModal() { this.showModal.set(false); this.resetGenForm(); }

  generateReport(): void {
    if (!this.genScope || !this.genScopeValue || !this.genFrom || !this.genTo) {
      this.flash('danger', 'All fields are required to generate a report.');
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
        this.loadAll();
      },
      error: () => {
        this.generating.set(false);
        this.flash('danger', 'Failed to generate report. Verify that data exists for the selected period.');
      },
    });
  }

  formatDate(d: string | null): string {
    return d ? new Date(d).toLocaleDateString('en-GB') : '—';
  }

  private resetGenForm(): void {
    this.genScope = '';  this.genScopeValue = '';
    this.genFrom  = '';  this.genTo         = '';
  }

  private toArr<T>(val: T[] | any): T[] {
    if (Array.isArray(val)) return val;
    if (val && Array.isArray((val as any).data)) return (val as any).data;
    return [];
  }

  private flash(type: string, text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
