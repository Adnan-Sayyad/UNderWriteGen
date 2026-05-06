import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { UnderwritingApiService } from '../../services/underwriting-api.service';
import { Subjectivity, SubjectivityStatus } from '../../models/underwriting.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

@Component({
  selector: 'app-subjectivity-list',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './subjectivity-list.html',
  styleUrl: './subjectivity-list.css',
})
export class SubjectivityListPage implements OnInit {
  readonly subjectivities = signal<Subjectivity[]>([]);
  readonly loading        = signal(false);
  readonly filterStatus   = signal('');
  readonly alertMsg       = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return this.subjectivities().filter(sub => !s || sub.status === s);
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Workbench', route: '/underwriting/workbench' },
    { label: 'Subjectivities' },
  ];

  constructor(private svc: UnderwritingApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getSubjectivities(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.subjectivities.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  updateStatus(s: Subjectivity, status: SubjectivityStatus): void {
    this.svc.updateSubjectivity(s.subjectivityId, { status }).subscribe({
      next: () => { this.load(); this.flash('success', `Marked as ${status}.`); },
      error: err => this.flash('danger', err?.error?.message ?? 'Update failed.'),
    });
  }

  statusClass(s: string): string {
    return s === 'Met' ? 'bg-success' : s === 'Waived' ? 'bg-secondary' : 'bg-warning text-dark';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
