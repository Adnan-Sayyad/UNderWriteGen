import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { ReportsNav } from '../../components/reports-nav/reports-nav';
import { ReportsApiService } from '../../services/reports-api.service';
import { ReportDetailDto } from '../../models/reports.model';

@Component({
  selector: 'app-report-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, ReportsNav],
  templateUrl: './report-detail.html',
  styleUrl: './report-detail.css',
})
export class ReportDetailPage implements OnInit {
  readonly reportId = signal('');
  readonly report   = signal<ReportDetailDto | null>(null);
  readonly loading  = signal(true);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Reports', route: '/reports' },
    { label: 'Report Detail' },
  ];

  constructor(
    private svc: ReportsApiService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.reportId.set(id);
    if (!id) { this.loading.set(false); return; }

    this.svc.getReport(id).subscribe({
      next: (res: ReportDetailDto | { data: ReportDetailDto }) => {
        // Handle both direct response and wrapped response gracefully
        const detail = 'data' in res && res.data ? res.data : res as ReportDetailDto;
        this.report.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.flash('danger', 'Failed to load report. It may not exist or the service may be unavailable.');
      },
    });
  }

  get r(): ReportDetailDto { return this.report()!; }

  riskPct(count: number): number {
    const total = this.r?.riskMix?.total ?? 0;
    return total > 0 ? Math.round((count / total) * 100) : 0;
  }

  // TAT bar widths — clamped to [0, 100] to prevent negative values
  get tatMinBarPct(): number {
    if (!this.r || this.r.taT_MaxHours <= 0) return 0;
    return Math.min(Math.max(Math.round((this.r.taT_MinHours / this.r.taT_MaxHours) * 100), 0), 100);
  }

  get tatAvgBarPct(): number {
    if (!this.r || this.r.taT_MaxHours <= 0) return 0;
    const range = this.r.taT_AvgHours - this.r.taT_MinHours;
    return Math.min(Math.max(Math.round((range / this.r.taT_MaxHours) * 100), 0), 100);
  }

  // Safe date formatter — returns — on invalid or null input
  formatDate(d: string | null | undefined): string {
    if (!d) return '—';
    try {
      const dt = new Date(d);
      return isNaN(dt.getTime())
        ? '—'
        : dt.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch {
      return '—';
    }
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
