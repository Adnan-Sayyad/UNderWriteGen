import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { UnderwritingApiService } from '../../services/underwriting-api.service';
import { Subjectivity, SubjectivityStatus } from '../../models/underwriting.model';

@Component({
  selector: 'app-subjectivity-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './subjectivity-list.html',
  styleUrl: './subjectivity-list.css',
})
export class SubjectivityListPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly subjectivities = signal<Subjectivity[]>([]);
  readonly loading        = signal(false);
  readonly filterStatus   = signal('');
  readonly searchQuery    = signal('');
  readonly alertMsg       = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly currentPage    = signal(0);
  readonly pageSize       = signal(10);
  readonly totalPages     = signal(0);
  readonly totalElements  = signal(0);
  readonly showCreateModal = signal(false);
  readonly creating        = signal(false);
  readonly confirmId       = signal<string | null>(null);
  readonly confirmAction   = signal<SubjectivityStatus | null>(null);

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    const q = this.searchQuery().toLowerCase();
    return this.subjectivities().filter(sub =>
      (!s || sub.status === s) &&
      (!q || sub.submissionId.toLowerCase().includes(q) || sub.description.toLowerCase().includes(q))
    );
  });

  readonly pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i)
  );

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Workbench', route: '/underwriting/workbench' },
    { label: 'Subjectivities' },
  ];

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    description:  ['', [Validators.required, Validators.minLength(10)]],
    dueDate:      ['', Validators.required],
  });

  constructor(private svc: UnderwritingApiService) {}

  ngOnInit(): void { this.load(); }

  load(page = this.currentPage()): void {
    this.loading.set(true);
    const req: any = { page, size: this.pageSize(), sort: 'dueDate', direction: 'asc' };
    this.svc.getSubjectivities(req).subscribe({
      next: (res: any) => {
        this.subjectivities.set(res?.content ?? res?.data ?? []);
        this.totalPages.set(res?.totalPages ?? 1);
        this.totalElements.set(res?.totalElements ?? 0);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

  askConfirm(s: Subjectivity, action: SubjectivityStatus): void {
    this.confirmId.set(s.subjectivityId);
    this.confirmAction.set(action);
  }

  cancelConfirm(): void {
    this.confirmId.set(null);
    this.confirmAction.set(null);
  }

  confirmUpdate(): void {
    const id = this.confirmId();
    const action = this.confirmAction();
    if (!id || !action) return;
    const sub = this.subjectivities().find(s => s.subjectivityId === id);
    if (!sub) return;
    this.cancelConfirm();
    this.svc.updateSubjectivity(id, { status: action }).subscribe({
      next: () => { this.load(this.currentPage()); this.flash('success', `Marked as ${action}.`); },
      error: err => this.flash('danger', err?.error?.message ?? 'Update failed.'),
    });
  }

  openCreateModal(): void {
    this.createForm.reset();
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void { this.showCreateModal.set(false); }

  saveSubjectivity(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    const v = this.createForm.value;
    const payload: Omit<Subjectivity, 'subjectivityId'> = {
      submissionId: v.submissionId!,
      description:  v.description!,
      dueDate:      v.dueDate!,
      status:       'Open',
    };
    this.creating.set(true);
    this.svc.createSubjectivity(payload).subscribe({
      next: () => {
        this.creating.set(false);
        this.closeCreateModal();
        this.flash('success', 'Subjectivity created.');
        this.load(0);
      },
      error: (err: any) => {
        this.creating.set(false);
        this.flash('danger', err?.error?.message ?? 'Failed to create subjectivity.');
      },
    });
  }

  statusClass(s: string): string {
    return s === 'Met' ? 'bg-success' : s === 'Waived' ? 'bg-secondary' : 'bg-warning text-dark';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
