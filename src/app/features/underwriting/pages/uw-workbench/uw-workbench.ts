import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { Submission } from '../../../submission/models/submission.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

@Component({
  selector: 'app-uw-workbench',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './uw-workbench.html',
  styleUrl: './uw-workbench.css',
})
export class UwWorkbenchPage implements OnInit {
  readonly submissions  = signal<Submission[]>([]);
  readonly loading      = signal(false);
  readonly searchQuery  = signal('');

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

  load(): void {
    this.loading.set(true);
    this.svc.getAll(DEFAULT_PAGE_REQUEST, { status: 'UnderReview' }).subscribe({
      next: res => {
        const d: any = res;
        this.submissions.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }
}
