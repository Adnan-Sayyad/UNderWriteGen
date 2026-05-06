import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, Attachment } from '../../models/submission.model';

type TabId = 'overview' | 'attachments' | 'completeness';

@Component({
  selector: 'app-submission-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './submission-detail.html',
  styleUrl: './submission-detail.css',
})
export class SubmissionDetailPage implements OnInit {
  readonly submission  = signal<Submission | null>(null);
  readonly attachments = signal<Attachment[]>([]);
  readonly loading     = signal(false);
  readonly activeTab   = signal<TabId>('overview');

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'Detail' },
  ];

  constructor(private svc: SubmissionApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.svc.getById(id).subscribe({
      next: res => {
        const d: any = res;
        this.submission.set(d?.data ?? d);
        this.loading.set(false);
        this.loadAttachments(id);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadAttachments(id: string): void {
    this.svc.getAttachments(id).subscribe({
      next: res => {
        const d: any = res;
        this.attachments.set(d?.data ?? []);
      },
      error: () => {},
    });
  }

  setTab(tab: TabId): void { this.activeTab.set(tab); }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark',
      UnderReview: 'bg-warning text-dark', Quoted: 'bg-primary',
      Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[status] ?? 'bg-secondary';
  }
}
