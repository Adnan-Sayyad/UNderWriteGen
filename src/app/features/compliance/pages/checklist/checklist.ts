import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ComplianceApiService } from '../../services/compliance-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { Submission } from '../../../submission/models/submission.model';
import {
  ComplianceChecklist, ChecklistStatus,
  CreateChecklistPayload, UpdateChecklistStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'view' | 'status' | null;
const STATUSES: ChecklistStatus[] = ['Pending', 'InProgress', 'Completed'];

@Component({
  selector: 'app-checklist',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './checklist.html',
  styleUrl: './checklist.css',
})
export class ChecklistPage implements OnInit {
  readonly statuses     = STATUSES;
  readonly checklists   = signal<ComplianceChecklist[]>([]);
  readonly loading      = signal(false);
  readonly saving       = signal(false);
  readonly modalMode    = signal<ModalMode>(null);
  readonly selected     = signal<ComplianceChecklist | null>(null);
  readonly filterStatus = signal('');
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  setFilterStatus(val: string): void { this.filterStatus.set(val); this.currentPage.set(1); }

  // ── Submission lookup (create form) ──────────────────────────
  readonly lookupLoading     = signal(false);
  readonly lookupSub         = signal<Submission | null>(null);
  readonly lookupError       = signal('');
  /** true only when the submission API confirmed the ID does not exist */
  readonly submissionInvalid = signal(false);

  // ── Submission detail (view modal) ───────────────────────────
  readonly detailSub        = signal<Submission | null>(null);
  readonly detailSubLoading = signal(false);

  // ── Summary counts ────────────────────────────────────────────
  readonly pendingCount    = computed(() => this.checklists().filter(c => c.status === 'Pending').length);
  readonly inProgressCount = computed(() => this.checklists().filter(c => c.status === 'InProgress').length);
  readonly completedCount  = computed(() => this.checklists().filter(c => c.status === 'Completed').length);

  // ── Filtered list ─────────────────────────────────────────────
  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return s ? this.checklists().filter(c => c.status === s) : this.checklists();
  });

  // ── Pagination ────────────────────────────────────────────────
  readonly currentPage = signal(1);
  readonly pageSize    = 10;
  readonly totalPages  = computed(() => Math.max(1, Math.ceil(this.filtered().length / this.pageSize)));
  readonly paged       = computed(() => {
    const page  = Math.min(this.currentPage(), this.totalPages());
    const start = (page - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  });
  readonly pageNumbers = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));

  prevPage(): void  { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage(): void  { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
  goToPage(n: number): void { this.currentPage.set(n); }

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Checklists' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    completedBy:  [''],
    status:       ['Pending' as ChecklistStatus, Validators.required],
  });

  readonly statusForm = this.fb.group({
    status: ['Pending' as ChecklistStatus, Validators.required],
  });

  /* checklist items managed locally before serialising to JSON */
  readonly items = signal<{ item: string; checked: boolean }[]>([]);
  newItemText = '';

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
      this.createForm.reset({ status: 'Pending' });
      this.createForm.patchValue({ submissionId: prefilledId });
      this.items.set([]);
      this.newItemText = '';
      this.lookupSub.set(null);
      this.lookupError.set('');
      this.submissionInvalid.set(false);
      this.modalMode.set('create');
      setTimeout(() => this.lookupSubmission(), 100);
    }
  }

  load(): void {
    this.loading.set(true);
    this.svc.getChecklists().subscribe({
      next: data => { this.checklists.set(data); this.loading.set(false); },
      error: err  => {
        this.loading.set(false);
        this.flash('danger', this.extractError(err, 'Failed to load compliance checklists.'));
      },
    });
  }

  openCreate(): void {
    this.createForm.reset({ status: 'Pending' });
    this.items.set([]);
    this.newItemText = '';
    this.lookupSub.set(null);
    this.lookupError.set('');
    this.submissionInvalid.set(false);
    this.modalMode.set('create');
  }

  openView(c: ComplianceChecklist): void {
    this.selected.set(c);
    try { this.items.set(JSON.parse(c.itemsJson)); } catch { this.items.set([]); }
    this.detailSub.set(null);
    this.detailSubLoading.set(true);
    this.submissionSvc.getById(c.submissionId).subscribe({
      next: r  => { this.detailSub.set(r.data); this.detailSubLoading.set(false); },
      error: () => this.detailSubLoading.set(false),
    });
    this.modalMode.set('view');
  }

  openStatus(c: ComplianceChecklist): void {
    this.selected.set(c);
    this.statusForm.patchValue({ status: c.status });
    this.modalMode.set('status');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.selected.set(null);
    this.items.set([]);
    this.newItemText = '';
    this.itemsError.set('');
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

  addItem(): void {
    const t = this.newItemText.trim();
    if (!t) return;
    this.items.update(list => [...list, { item: t, checked: false }]);
    this.newItemText = '';
    this.itemsError.set('');   // clear validation error once at least one item exists
  }

  toggleItem(idx: number): void {
    this.items.update(list =>
      list.map((it, i) => i === idx ? { ...it, checked: !it.checked } : it)
    );
  }

  removeItem(idx: number): void {
    this.items.update(list => list.filter((_, i) => i !== idx));
  }

  readonly itemsError = signal('');

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    if (this.items().length === 0) {
      this.itemsError.set('Please add at least one checklist item before saving.');
      return;
    }
    this.itemsError.set('');
    this.saving.set(true);
    const v = this.createForm.value;
    const payload: CreateChecklistPayload = {
      submissionId: v.submissionId!,
      itemsJson:    JSON.stringify(this.items()),
      completedBy:  v.completedBy || undefined,
      status:       v.status as ChecklistStatus,
    };
    this.svc.createChecklist(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Compliance checklist created successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', this.extractError(err, 'Failed to create the checklist.'));
      },
    });
  }

  saveStatus(): void {
    if (!this.selected()) return;
    this.saving.set(true);
    const payload: UpdateChecklistStatusPayload = { status: this.statusForm.value.status as ChecklistStatus };
    this.svc.updateChecklistStatus(this.selected()!.checklistId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Checklist status updated successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', this.extractError(err, 'Failed to update checklist status.'));
      },
    });
  }

  progressOf(c: ComplianceChecklist): { done: number; total: number; pct: number } {
    try {
      const items: { checked: boolean }[] = JSON.parse(c.itemsJson);
      const done = items.filter(i => i.checked).length;
      return { done, total: items.length, pct: items.length ? Math.round((done / items.length) * 100) : 0 };
    } catch { return { done: 0, total: 0, pct: 0 }; }
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
