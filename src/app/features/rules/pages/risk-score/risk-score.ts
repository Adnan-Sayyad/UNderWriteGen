import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import {
  RiskScore, RiskBand,
  RISK_BANDS,
} from '../../models/rules.model';

@Component({
  selector: 'app-risk-score',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './risk-score.html',
  styleUrl: './risk-score.css',
})
export class RiskScorePage {
  private readonly fb = inject(FormBuilder);

  readonly submissionId = signal('');
  readonly latest       = signal<RiskScore | null>(null);
  readonly history      = signal<RiskScore[]>([]);
  readonly bandFilter   = signal<RiskBand | ''>('');
  readonly bandScores   = signal<RiskScore[]>([]);
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

  search(): void {
    if (this.searchForm.invalid) { this.searchForm.markAllAsTouched(); return; }
    const id = this.searchForm.value.submissionId!.trim();
    this.submissionId.set(id);
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
        this.history.update(list => [score, ...list]);
        this.flash('success', `Score ${score.scoreValue} (${score.band}) calculated.`);
      },
      error: () => { this.calculating.set(false); this.flash('danger', 'Failed to calculate score.'); },
    });
  }

  filterByBand(): void {
    const band = this.bandFilter();
    if (!band) { this.bandScores.set([]); return; }
    this.bandLoading.set(true);
    this.svc.getScoresByBand(band).subscribe({
      next: r => { this.bandScores.set(r); this.bandLoading.set(false); },
      error: () => { this.bandLoading.set(false); this.flash('danger', 'Failed to load band scores.'); },
    });
  }

  bandClass(b: RiskBand): string {
    return b === 'High' ? 'bg-danger' : b === 'Medium' ? 'bg-warning text-dark' : 'bg-success';
  }

  scoreGaugeColor(value: number): string {
    if (value >= 75) return 'var(--uwpro-danger, #d93025)';
    if (value >= 50) return 'var(--uwpro-warning, #e6a817)';
    return 'var(--uwpro-success, #1e7e34)';
  }

  scorePercent(value: number): number {
    return Math.max(0, Math.min(100, value));
  }

  private loadForSubmission(id: string): void {
    this.loading.set(true);
    this.latest.set(null);
    this.history.set([]);
    this.svc.getLatestRiskScore(id).subscribe({
      next: score => this.latest.set(score),
      error: () => this.latest.set(null),
    });
    this.svc.getRiskScoreHistory(id).subscribe({
      next: list => { this.history.set(list); this.loading.set(false); },
      error: () => { this.loading.set(false); },
    });
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
