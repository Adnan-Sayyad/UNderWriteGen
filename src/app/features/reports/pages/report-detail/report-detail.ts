import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { ReportsApiService } from '../../services/reports-api.service';
import { UWReport } from '../../models/reports.model';

@Component({
  selector: 'app-report-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader],
  templateUrl: './report-detail.html',
  styleUrl: './report-detail.css',
})
export class ReportDetailPage implements OnInit {
  readonly reportId = signal('');
  readonly report   = signal<UWReport | null>(null);
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

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.reportId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.svc.getReport(id).subscribe({
      next: (res: any) => { this.report.set(res?.data ?? res); this.loading.set(false); },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load report.'); },
    });
  }

  metricsEntries(): { key: string; value: string }[] {
    const m = this.report()?.metrics ?? {};
    return Object.entries(m).map(([key, value]) => ({ key, value: String(value) }));
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
