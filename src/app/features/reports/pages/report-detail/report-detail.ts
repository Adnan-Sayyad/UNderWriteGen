import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { ReportsApiService } from '../../services/reports-api.service';
import { ReportDetailDto } from '../../models/reports.model';

@Component({
  selector: 'app-report-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader],
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
      next: (res: any) => {
        this.report.set(res?.data ?? res);
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

  formatDate(d: string | null): string {
    return d ? new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '—';
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
