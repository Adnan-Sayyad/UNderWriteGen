import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, SubmissionStatus, ProductLine } from '../../models/submission.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

const STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];
const PRODUCT_LINES: ProductLine[] = ['Life', 'Health', 'PnC', 'Commercial'];

@Component({
  selector: 'app-submission-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './submission-list.html',
  styleUrl: './submission-list.css',
})
export class SubmissionListPage implements OnInit {
  readonly submissions  = signal<Submission[]>([]);
  readonly loading      = signal(false);
  readonly searchQuery  = signal('');
  readonly filterStatus = signal('');
  readonly filterProduct = signal('');

  readonly statuses    = STATUSES;
  readonly productLines = PRODUCT_LINES;

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    const p = this.filterProduct();
    return this.submissions().filter(sub =>
      (!q || sub.submissionId.toLowerCase().includes(q) || sub.partyId?.toLowerCase().includes(q)) &&
      (!s || sub.status === s) &&
      (!p || sub.productLine === p)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions' },
  ];

  constructor(private svc: SubmissionApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const data: any = res;
        this.submissions.set(data?.content ?? data?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark',
      UnderReview: 'bg-warning text-dark', Quoted: 'bg-primary',
      Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[status] ?? 'bg-secondary';
  }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }
}
