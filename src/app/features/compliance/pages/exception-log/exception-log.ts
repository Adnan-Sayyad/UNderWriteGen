import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ComplianceApiService } from '../../services/compliance-api.service';
import {
  ExceptionLog, ExceptionCategory, ExceptionStatus,
  CreateExceptionPayload, UpdateExceptionStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'detail' | null;

const CATEGORIES: ExceptionCategory[] = ['Data', 'Process', 'Compliance'];

@Component({
  selector: 'app-exception-log',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './exception-log.html',
  styleUrl: './exception-log.css',
})
export class ExceptionLogPage implements OnInit {
  readonly categories    = CATEGORIES;
  readonly exceptions    = signal<ExceptionLog[]>([]);
  readonly loading       = signal(false);
  readonly saving        = signal(false);
  readonly modalMode     = signal<ModalMode>(null);
  readonly selected      = signal<ExceptionLog | null>(null);
  readonly filterCat     = signal('');
  readonly filterStatus  = signal('');
  readonly alertMsg      = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly filtered = computed(() => {
    const c = this.filterCat();
    const s = this.filterStatus();
    return this.exceptions().filter(e =>
      (!c || e.category === c) && (!s || e.status === s)
    );
  });

  readonly openCount   = computed(() => this.exceptions().filter(e => e.status === 'Open').length);
  readonly closedCount = computed(() => this.exceptions().filter(e => e.status === 'Closed').length);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Exception Log' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    category:     ['Data' as ExceptionCategory, Validators.required],
    details:      ['', [Validators.required, Validators.minLength(10)]],
  });

  constructor(private svc: ComplianceApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getExceptions().subscribe({
      next: data => { this.exceptions.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  openCreate(): void { this.createForm.reset({ category: 'Data' }); this.modalMode.set('create'); }
  openDetail(e: ExceptionLog): void { this.selected.set(e); this.modalMode.set('detail'); }
  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.value;
    const payload: CreateExceptionPayload = {
      submissionId: v.submissionId!,
      category:     v.category as ExceptionCategory,
      details:      v.details!,
    };
    this.svc.createException(payload).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Exception logged.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Create failed.'); },
    });
  }

  toggleClose(e: ExceptionLog): void {
    const newStatus: ExceptionStatus = e.status === 'Open' ? 'Closed' : 'Open';
    const payload: UpdateExceptionStatusPayload = { status: newStatus };
    this.svc.updateExceptionStatus(e.exceptionId, payload).subscribe({
      next: () => { this.load(); this.flash('success', `Exception marked as ${newStatus}.`); },
      error: err => this.flash('danger', err?.error?.message ?? 'Update failed.'),
    });
  }

  categoryClass(cat: string): string {
    return cat === 'Compliance' ? 'bg-danger' : cat === 'Process' ? 'bg-warning text-dark' : 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
