import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { Submission } from '../../../submission/models/submission.model';

@Component({
  selector: 'app-uw-workbench',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './uw-workbench.html',
  styleUrl: './uw-workbench.css',
})
export class UwWorkbenchPage implements OnInit {
  readonly submissions   = signal<Submission[]>([]);
  readonly loading       = signal(false);
  readonly searchQuery   = signal('');
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.submissions().filter(s =>
      !q || s.submissionId.toLowerCase().includes(q) || s.productLine.toLowerCase().includes(q)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Underwriting', route: '/underwriting' },
    { label: 'Workbench' },
  ];

  constructor(private svc: SubmissionApiService) {}

  ngOnInit(): void { this.load(); }

  load(page = this.currentPage()): void {
    this.loading.set(true);
    const req: any = { page, size: this.pageSize(), sort: 'createdDate', direction: 'desc' };
    this.svc.getAll(req).subscribe({
      next: res => {
        // Show only submissions that are UnderReview or Quoted (active UW work)
        const all = res.content ?? [];
        const uwItems = all.filter((s: any) =>
          s.status === 'UnderReview' || s.status === 'IntakeComplete' || s.status === 'Quoted'
        );
        this.submissions.set(uwItems);
        this.totalPages.set(res.totalPages ?? 1);
        this.totalElements.set(uwItems.length);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }
}
