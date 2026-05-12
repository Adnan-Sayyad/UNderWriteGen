import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ComplianceApiService } from '../../services/compliance-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { Submission } from '../../../submission/models/submission.model';
import {
  AuthorityBreach, BreachType, BreachStatus,
  CreateBreachPayload, UpdateBreachStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'detail' | 'review' | 'edit' | null;

const BREACH_TYPES: BreachType[]      = ['Authority', 'RuleOverride', 'PricingTolerance'];
const ALL_STATUSES: BreachStatus[]    = ['Pending', 'Approved', 'Rejected'];
const REVIEW_STATUSES: BreachStatus[] = ['Approved', 'Rejected'];

@Component({
  selector: 'app-authority-breaches',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './authority-breaches.html',
  styleUrl: './authority-breaches.css',
})
export class AuthorityBreachesPage implements OnInit {
  readonly breachTypes    = BREACH_TYPES;
  readonly allStatuses    = ALL_STATUSES;
  readonly reviewStatuses = REVIEW_STATUSES;

  readonly breaches     = signal<AuthorityBreach[]>([]);
  readonly loading      = signal(false);
  readonly saving       = signal(false);
  readonly modalMode    = signal<ModalMode>(null);
  readonly selected     = signal<AuthorityBreach | null>(null);
  readonly filterType   = signal('');
  readonly filterStatus = signal('');
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  /** Persistent load error — stays visible until the next successful load */
  readonly loadError    = signal<string | null>(null);
  /** Persistent modal error — shows inside the create/edit modal until dismissed */
  readonly modalError   = signal<string | null>(null);

  // ── Submission lookup (create form) ──────────────────────────
  readonly lookupLoading    = signal(false);
  readonly lookupSub        = signal<Submission | null>(null);
  readonly lookupError      = signal('');
  /** true only when the submission API confirmed the ID does not exist */
  readonly submissionInvalid = signal(false);

  // ── Submission detail (detail modal) ─────────────────────────
  readonly detailSub        = signal<Submission | null>(null);
  readonly detailSubLoading = signal(false);

  // ── Summary counts ────────────────────────────────────────────
  readonly pendingCount  = computed(() => this.breaches().filter(b => b.status === 'Pending').length);
  readonly approvedCount = computed(() => this.breaches().filter(b => b.status === 'Approved').length);
  readonly rejectedCount = computed(() => this.breaches().filter(b => b.status === 'Rejected').length);

  // ── Filtered list ─────────────────────────────────────────────
  readonly filtered = computed(() => {
    const t = this.filterType();
    const s = this.filterStatus();
    return this.breaches().filter(b =>
      (!t || b.breachType === t) && (!s || b.status === s)
    );
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
  setFilterType(val: string): void   { this.filterType.set(val);   this.currentPage.set(1); }
  setFilterStatus(val: string): void { this.filterStatus.set(val); this.currentPage.set(1); }

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Authority Breaches' },
  ];

  private readonly fb = inject(FormBuilder);

  // ── Create form ───────────────────────────────────────────────
  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    breachType:   ['Authority' as BreachType, Validators.required],
    description:  ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
  });

  // ── Review (approve / reject) form ───────────────────────────
  readonly reviewForm = this.fb.group({
    decision:   ['Approved' as BreachStatus, Validators.required],
    reviewedBy: ['', Validators.required],
  });

  // ── Edit details form (Status + ApprovedBy + ApprovedDate) ───
  readonly editForm = this.fb.group({
    status:      ['Pending' as BreachStatus, Validators.required],
    approvedBy:  [''],
    approvedDate: [''],
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
      this.createForm.reset({ breachType: 'Authority' });
      this.createForm.patchValue({ submissionId: prefilledId });
      this.lookupSub.set(null);
      this.lookupError.set('');
      this.submissionInvalid.set(false);
      this.modalMode.set('create');
      // Trigger the submission lookup after the form is patched
      setTimeout(() => this.lookupSubmission(), 100);
    }
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set(null);   // clear any previous load error
    console.log('[AuthorityBreaches] load() called — fetching from API');
    this.svc.getBreaches().subscribe({
      next: data  => {
        console.log('[AuthorityBreaches] load() got', data.length, 'records:', data);
        this.breaches.set(data);
        this.loading.set(false);
      },
      error: err  => {
        console.error('[AuthorityBreaches] load() error:', err);
        this.loading.set(false);
        // Persistent — stays until user hits Retry or navigates away
        this.loadError.set(this.extractError(err, 'Failed to load authority breaches.'));
      },
    });
  }

  // ── Modal openers ─────────────────────────────────────────────
  openCreate(): void {
    this.createForm.reset({ breachType: 'Authority' });
    this.lookupSub.set(null);
    this.lookupError.set('');
    this.submissionInvalid.set(false);
    this.modalMode.set('create');
  }

  openDetail(b: AuthorityBreach): void {
    this.selected.set(b);
    this.detailSub.set(null);
    this.detailSubLoading.set(true);
    this.submissionSvc.getById(b.submissionId).subscribe({
      next: r  => { this.detailSub.set(r.data); this.detailSubLoading.set(false); },
      error: () => this.detailSubLoading.set(false),
    });
    this.modalMode.set('detail');
  }

  openReview(b: AuthorityBreach): void {
    this.selected.set(b);
    const currentName = this.auth.currentUser()?.name ?? '';
    this.reviewForm.reset({ decision: 'Approved', reviewedBy: currentName });
    this.modalMode.set('review');
  }

  openEdit(b: AuthorityBreach): void {
    this.selected.set(b);
    // Format approvedDate as yyyy-MM-dd for the HTML date input
    const dateVal = b.approvedDate
      ? new Date(b.approvedDate).toISOString().split('T')[0]
      : '';
    this.editForm.reset({
      status:       b.status,
      approvedBy:   b.approvedBy ?? '',
      approvedDate: dateVal,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void {
    this.modalMode.set(null);
    this.selected.set(null);
    this.lookupSub.set(null);
    this.lookupError.set('');
    this.submissionInvalid.set(false);
    this.detailSub.set(null);
    this.modalError.set(null);
  }

  // ── Auto-lookup when user pastes a UUID ──────────────────────
  onSubmissionIdPaste(): void {
    // Wait one tick so the pasted value is written into the form control first
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

  // ── Save: create ──────────────────────────────────────────────
  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    // Guard: if the token is gone (logged out in another tab, expired) show a
    // clear message rather than letting the interceptor silently redirect to login.
    if (!this.auth.isAuthenticated()) {
      this.modalError.set(
        'Your session has expired. Please save your work, close this modal, and log in again.'
      );
      return;
    }
    this.saving.set(true);
    this.modalError.set(null);
    const v = this.createForm.value;
    const payload: CreateBreachPayload = {
      submissionId: v.submissionId!,
      breachType:   v.breachType as BreachType,
      description:  v.description!,
    };
    this.svc.createBreach(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Authority breach logged successfully.');
      },
      error: err => {
        this.saving.set(false);
        // Show inside the modal — does NOT auto-dismiss
        this.modalError.set(this.extractError(err, 'Failed to log the authority breach.'));
      },
    });
  }

  // ── Save: review (approve / reject) ──────────────────────────
  saveReview(): void {
    if (this.reviewForm.invalid) { this.reviewForm.markAllAsTouched(); return; }
    if (!this.selected()) return;
    if (!this.auth.isAuthenticated()) {
      this.modalError.set('Your session has expired. Please log in again to continue.');
      return;
    }
    this.saving.set(true);
    this.modalError.set(null);
    const v = this.reviewForm.value;
    const payload: UpdateBreachStatusPayload = {
      status:       v.decision as BreachStatus,
      approvedBy:   v.reviewedBy || undefined,
      approvedDate: new Date().toISOString(),
    };
    this.svc.updateBreachStatus(this.selected()!.breachId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', `Breach ${v.decision === 'Approved' ? 'approved' : 'rejected'} successfully.`);
      },
      error: err => {
        this.saving.set(false);
        this.modalError.set(this.extractError(err, 'Review submission failed.'));
      },
    });
  }

  // ── Save: edit details ────────────────────────────────────────
  saveEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    if (!this.selected()) return;
    if (!this.auth.isAuthenticated()) {
      this.modalError.set('Your session has expired. Please log in again to continue.');
      return;
    }
    this.saving.set(true);
    this.modalError.set(null);
    const v = this.editForm.value;
    const payload: UpdateBreachStatusPayload = {
      status:       v.status as BreachStatus,
      approvedBy:   v.approvedBy?.trim() || undefined,
      approvedDate: v.approvedDate
        ? new Date(v.approvedDate).toISOString()
        : undefined,
    };
    this.svc.updateBreachStatus(this.selected()!.breachId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Authority breach updated successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.modalError.set(this.extractError(err, 'Failed to update authority breach.'));
      },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────
  breachTypeClass(t: string): string {
    return t === 'Authority'
      ? 'bg-danger'
      : t === 'RuleOverride'
        ? 'bg-warning text-dark'
        : 'bg-info text-dark';
  }

  breachTypeLabel(t: string): string {
    return t === 'RuleOverride' ? 'Rule Override' : t === 'PricingTolerance' ? 'Pricing Tolerance' : t;
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

    // Parse the error body – Angular may give us a string if content-type was
    // not set correctly before the response started streaming.
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
    // Only auto-dismiss successes — errors stay until the user acts again
    if (type === 'success') {
      setTimeout(() => this.alertMsg.set(null), 5000);
    }
  }
}
