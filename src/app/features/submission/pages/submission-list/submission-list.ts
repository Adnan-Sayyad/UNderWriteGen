import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, SubmissionStatus, ProductLine } from '../../models/submission.model';
const STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];
const PRODUCT_LINES: ProductLine[] = ['Life', 'Health', 'PnC', 'Commercial'];

@Component({
  selector: 'app-submission-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './submission-list.html',
  styleUrl: './submission-list.css',
})
export class SubmissionListPage implements OnInit {
  readonly submissions   = signal<Submission[]>([]);
  readonly loading       = signal(false);
  readonly searchQuery   = signal('');
  readonly filterStatus  = signal('');
  readonly filterProduct = signal('');
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);

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

  constructor(private svc: SubmissionApiService, private router: Router) {}

  logBreach(submissionId: string): void {
    this.router.navigate(['/compliance/authority-breaches'], { queryParams: { submissionId } });
  }

  logException(submissionId: string): void {
    this.router.navigate(['/compliance/exceptions'], { queryParams: { submissionId } });
  }

  newChecklist(submissionId: string): void {
    this.router.navigate(['/compliance/checklists'], { queryParams: { submissionId } });
  }

  ngOnInit(): void { this.load(); }

  load(page = this.currentPage()): void {
    this.loading.set(true);
    const req: any = { page, size: this.pageSize(), sort: 'createdDate', direction: 'desc' };
    this.svc.getAll(req).subscribe({
      next: res => {
        this.submissions.set(res.content ?? []);
        this.totalPages.set(res.totalPages ?? 1);
        this.totalElements.set(res.totalElements ?? 0);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

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
