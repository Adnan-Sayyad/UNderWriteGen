import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { ComplianceApiService } from '../../services/compliance-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { Submission } from '../../../submission/models/submission.model';
import {
  ExceptionLog, ExceptionCategory, ExceptionStatus,
  CreateExceptionPayload, UpdateExceptionStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'detail' | null;

const CATEGORIES: ExceptionCategory[] = ['Data', 'Process', 'Compliance'];
const ALL_STATUSES: ExceptionStatus[]  = ['Open', 'Closed'];

@Component({
  selector: 'app-exception-log',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState, Pagination],
  templateUrl: './exception-log.html',
  styleUrl: './exception-log.css',
})
export class ExceptionLogPage implements OnInit {
  readonly categories  = CATEGORIES;
  readonly allStatuses = ALL_STATUSES;
  readonly exceptions  = signal<ExceptionLog[]>([]);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly modalMode   = signal<ModalMode>(null);
  readonly selected    = signal<ExceptionLog | null>(null);
  readonly filterCat   = signal('');
  readonly filterStatus = signal('');
  readonly alertMsg    = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  // ── Submission lookup (create form) ──────────────────────────
  readonly lookupLoading     = signal(false);
  readonly lookupSub         = signal<Submission | null>(null);
  readonly lookupError       = signal('');
  /** true only when the submission API confirmed the ID does not exist */
  readonly submissionInvalid = signal(false);

  // ── Submission detail (detail modal) ─────────────────────────
  readonly detailSub        = signal<Submission | null>(null);
  readonly detailSubLoading = signal(false);

  // ── Summary counts ────────────────────────────────────────────
  readonly openCount       = computed(() => this.exceptions().filter(e => e.status === 'Open').length);
  readonly closedCount     = computed(() => this.exceptions().filter(e => e.status === 'Closed').length);
  readonly dataCount       = computed(() => this.exceptions().filter(e => e.category === 'Data').length);
  readonly processCount    = computed(() => this.exceptions().filter(e => e.category === 'Process').length);
  readonly complianceCount = computed(() => this.exceptions().filter(e => e.category === 'Compliance').length);

  // ── Filtered list ─────────────────────────────────────────────
  readonly filtered = computed(() => {
    const c = this.filterCat();
    const s = this.filterStatus();
    return this.exceptions().filter(e =>
      (!c || e.category === c) && (!s || e.status === s)
    );
  });

  // ── Pagination ────────────────────────────────────────────────
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalElements = computed(() => this.filtered().length);
  readonly totalPages    = computed(() => Math.max(1, Math.ceil(this.totalElements() / this.pageSize())));
  readonly paged         = computed(() => {
    const size  = this.pageSize();
    const page  = Math.min(this.currentPage(), this.totalPages() - 1);
    const start = page * size;
    return this.filtered().slice(start, start + size);
  });

  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }
  setFilterCat(val: string): void    { this.filterCat.set(val);    this.currentPage.set(0); }
  setFilterStatus(val: string): void { this.filterStatus.set(val); this.currentPage.set(0); }

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Exception Log' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    category:     ['Data' as ExceptionCategory, Validators.required],
    details:      ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]],
  });

  constructor(
    private svc: ComplianceApiService,
    private submissionSvc: SubmissionApiService,
    readonly auth: AuthService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.load();
    // Pre-fill create modal when navigated from Submission page
    const prefilledId = this.route.snapshot.queryParamMap.get('submissionId');
    if (prefilledId && this.auth.hasRole('Admin', 'Compliance')) {
      this.createForm.reset({ category: 'Data' });
      this.createForm.patchValue({ submissionId: prefilledId });
      this.lookupSub.set(null);
      this.lookupError.set('');
      this.submissionInvalid.set(false);
      this.modalMode.set('create');
      setTimeout(() => this.lookupSubmission(), 100);
    }
  }

  load(): void {
    this.loading.set(true);
    this.svc.getExceptions().subscribe({
      next: data => { this.exceptions.set(data); this.loading.set(false); },
      error: err  => {
        this.loading.set(false);
        this.flash('danger', this.extractError(err, 'Failed to load exception logs.'));
      },
    });
  }

  openCreate(): void {
    this.createForm.reset({ category: 'Data' });
    this.lookupSub.set(null);
    this.lookupError.set('');
    this.submissionInvalid.set(false);
    this.modalMode.set('create');
  }

  openDetail(e: ExceptionLog): void {
    this.selected.set(e);
    this.detailSub.set(null);
    this.detailSubLoading.set(true);
    this.submissionSvc.getById(e.submissionId).subscribe({
      next: r  => { this.detailSub.set(r.data); this.detailSubLoading.set(false); },
      error: () => this.detailSubLoading.set(false),
    });
    this.modalMode.set('detail');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.selected.set(null);
    this.lookupSub.set(null);
    this.lookupError.set('');
    this.submissionInvalid.set(false);
    this.detailSub.set(null);
  }

  // ── Auto-lookup when user pastes a UUID ──────────────────────
  onSubmissionIdPaste(): void {
    setTimeout(() => this.lookupSubmission(), 50);
  }

  // ── Submission lookup ─────────────────────────────────────────
  lookupSubmission(): void {
    const id = this.createForm.controls.submissionId.value?.trim();
    if (!id) return;
    this.lookupLoading.set(true);
    this.lookupError.set('');
    this.lookupSub.set(null);
    this.submissionInvalid.set(false);
    this.submissionSvc.getById(id).subscribe({
      next: r  => {
        this.lookupSub.set(r.data);
        this.lookupLoading.set(false);
        this.submissionInvalid.set(false);
      },
      error: err => {
        this.lookupLoading.set(false);
        if (err?.status === 404) {
          this.lookupError.set('Submission not found. Please check the UUID and try again.');
          this.submissionInvalid.set(true);
        } else if (err?.status === 0) {
          this.lookupError.set('Submission API is unreachable. The backend will validate the ID on save.');
          this.submissionInvalid.set(false);
        } else {
          this.lookupError.set('Lookup failed. Please verify the submission ID.');
          this.submissionInvalid.set(false);
        }
      },
    });
  }

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
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Exception logged successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', this.extractError(err, 'Failed to log the exception.'));
      },
    });
  }

  toggleClose(e: ExceptionLog): void {
    const newStatus: ExceptionStatus = e.status === 'Open' ? 'Closed' : 'Open';
    const payload: UpdateExceptionStatusPayload = { status: newStatus };
    this.svc.updateExceptionStatus(e.exceptionId, payload).subscribe({
      next: () => {
        this.load();
        this.flash('success', `Exception marked as ${newStatus}.`);
      },
      error: err => this.flash('danger', this.extractError(err, 'Failed to update exception status.')),
    });
  }

  categoryClass(cat: string): string {
    return cat === 'Compliance'
      ? 'bg-danger'
      : cat === 'Process'
        ? 'bg-warning text-dark'
        : 'bg-secondary';
  }

  productLineClass(pl: string): string {
    const map: Record<string, string> = {
      Life: 'bg-primary', Health: 'bg-success', PnC: 'bg-info text-dark', Commercial: 'bg-warning text-dark',
    };
    return map[pl] ?? 'bg-secondary';
  }

  subStatusClass(st: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark', UnderReview: 'bg-primary',
      Quoted: 'bg-success', Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[st] ?? 'bg-secondary';
  }

  // ── Extract a readable message from any HttpErrorResponse ────
  private extractError(err: any, fallback: string): string {
    if (err?.status === 0)
      return 'Cannot connect to the server. Please make sure the Compliance API is running on port 8089.';
    if (err?.status === 401)
      return 'Your session has expired. Please log in again.';
    if (err?.status === 403)
      return 'Access denied. You do not have permission to perform this action.';

    // Angular may deliver the body as a raw string when content-type is ambiguous
    let body = err?.error;
    if (typeof body === 'string') {
      try { body = JSON.parse(body); } catch { /* leave as string */ }
    }

    if (err?.status === 404)
      return (typeof body === 'object' ? body?.message ?? body?.Message : null)
          ?? 'Record not found. Please check the ID and try again.';

    if (body?.errors) {
      const first = Object.values(body.errors as Record<string, string[]>)[0];
      return Array.isArray(first) ? first[0] : fallback;
    }

    return body?.message
        ?? body?.Message
        ?? body?.title
        ?? fallback;
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
