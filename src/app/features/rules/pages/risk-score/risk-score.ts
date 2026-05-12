import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pager } from '../../components/pager/pager';
import { RulesApiService } from '../../services/rules-api.service';
import {
  RiskScore, RiskBand,
  RISK_BANDS, DEFAULT_PAGE_SIZE,
} from '../../models/rules.model';

@Component({
  selector: 'app-risk-score',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState, Pager],
  templateUrl: './risk-score.html',
  styleUrl: './risk-score.css',
})
export class RiskScorePage {
  private readonly fb = inject(FormBuilder);

  readonly submissionId = signal('');
  readonly latest       = signal<RiskScore | null>(null);

  // History (paginated)
  readonly history             = signal<RiskScore[]>([]);
  readonly historyPage         = signal(0);
  readonly historySize         = signal(10);
  readonly historyTotal        = signal(0);
  readonly historyTotalPages   = signal(0);

  // Band browse (paginated)
  readonly bandFilter         = signal<RiskBand | ''>('');
  readonly bandScores         = signal<RiskScore[]>([]);
  readonly bandPage           = signal(0);
  readonly bandSize           = signal(DEFAULT_PAGE_SIZE);
  readonly bandTotal          = signal(0);
  readonly bandTotalPages     = signal(0);

  readonly loading      = signal(false);
  readonly bandLoading  = signal(false);
  readonly calculating  = signal(false);
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly bands = RISK_BANDS;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Rules', route: '/rules/list' },
    { label: 'Risk Scoring' },
  ];

  readonly searchForm = this.fb.group({
    submissionId: ['', Validators.required],
  });

  constructor(private svc: RulesApiService) {}

  // ── Submission lookup ─────────────────────────────────────────────────────
  search(): void {
    if (this.searchForm.invalid) { this.searchForm.markAllAsTouched(); return; }
    const id = (this.searchForm.value.submissionId ?? '').trim();
    if (!id) { this.flash('danger', 'Please enter a submission id.'); return; }
    this.submissionId.set(id);
    this.historyPage.set(0);
    this.loadForSubmission(id);
  }

  calculate(): void {
    const id = this.submissionId();
    if (!id) return;
    this.calculating.set(true);
    this.svc.calculateRiskScore(id).subscribe({
      next: score => {
        this.calculating.set(false);
        this.latest.set(score);
        this.historyPage.set(0);
        this.loadHistory(id);
        this.flash('success', `Score ${score.scoreValue.toFixed(1)} (${score.band}) calculated.`);
      },
      error: () => { this.calculating.set(false); this.flash('danger', 'Failed to calculate score.'); },
    });
  }

  onHistoryPageChange(p: number) {
    if (!this.submissionId()) return;
    this.historyPage.set(p);
    this.loadHistory(this.submissionId());
  }
  onHistorySizeChange(s: number) {
    if (!this.submissionId()) return;
    this.historySize.set(s);
    this.historyPage.set(0);
    this.loadHistory(this.submissionId());
  }

  // ── Band browse ───────────────────────────────────────────────────────────
  filterByBand(): void {
    const band = this.bandFilter();
    if (!band) { this.bandScores.set([]); this.bandTotal.set(0); this.bandTotalPages.set(0); return; }
    this.bandPage.set(0);
    this.loadBandScores();
  }

  onBandPageChange(p: number) { this.bandPage.set(p); this.loadBandScores(); }
  onBandSizeChange(s: number) { this.bandSize.set(s); this.bandPage.set(0); this.loadBandScores(); }

  private loadBandScores(): void {
    const band = this.bandFilter();
    if (!band) return;
    this.bandLoading.set(true);
    this.svc.getScoresByBand(band, this.bandPage(), this.bandSize()).subscribe({
      next: r => {
        this.bandScores.set(r.content);
        this.bandTotal.set(r.totalElements);
        this.bandTotalPages.set(r.totalPages);
        this.bandPage.set(r.page);
        this.bandLoading.set(false);
      },
      error: () => { this.bandLoading.set(false); this.flash('danger', 'Failed to load band scores.'); },
    });
  }

  // ── Loaders ───────────────────────────────────────────────────────────────
  private loadForSubmission(id: string): void {
    this.loading.set(true);
    this.latest.set(null);
    this.history.set([]);
    this.svc.getLatestRiskScore(id).subscribe({
      next: score => this.latest.set(score),
      error: () => this.latest.set(null),
    });
    this.loadHistory(id);
  }

  private loadHistory(id: string): void {
    this.loading.set(true);
    this.svc.getRiskScoreHistory(id, this.historyPage(), this.historySize()).subscribe({
      next: r => {
        this.history.set(r.content);
        this.historyTotal.set(r.totalElements);
        this.historyTotalPages.set(r.totalPages);
        this.historyPage.set(r.page);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); },
    });
  }

  // ── Visuals ───────────────────────────────────────────────────────────────
  bandClass(b: RiskBand): string {
    return b === 'High' ? 'bg-danger' : b === 'Medium' ? 'bg-warning text-dark' : 'bg-success';
  }

  scoreGaugeColor(value: number): string {
    if (value >= 75) return '#d93025';
    if (value >= 50) return '#e6a817';
    return '#1e7e34';
  }

  scorePercent(value: number): number {
    return Math.max(0, Math.min(100, value));
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
