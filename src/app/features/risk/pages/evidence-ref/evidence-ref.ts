import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RiskApiService } from '../../services/risk-api.service';
import { EvidenceRef, EvidenceType, EvidenceStatus } from '../../models/risk.model';

const EVIDENCE_TYPES: EvidenceType[] = ['InspectionReport', 'Medical', 'Lab', 'Telematics', 'ClaimsHistory', 'Sanctions'];
const EVIDENCE_STATUSES: EvidenceStatus[] = ['Requested', 'Received', 'NotAvailable'];

@Component({
  selector: 'app-evidence-ref',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './evidence-ref.html',
  styleUrl: './evidence-ref.css',
})
export class EvidenceRefPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly evidence      = signal<EvidenceRef[]>([]);
  readonly loading       = signal(false);
  readonly saving        = signal(false);
  readonly showModal     = signal(false);
  readonly submissionId  = signal('');
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly evidenceTypes = EVIDENCE_TYPES;
  readonly statuses      = EVIDENCE_STATUSES;

  readonly updatingId    = signal<string | null>(null);
  readonly deletingId    = signal<string | null>(null);
  readonly editingId     = signal<string | null>(null);
  readonly editStatus    = signal<EvidenceStatus>('Requested');

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Risk & Evidence', route: '/risk' },
    { label: 'Evidence' },
  ];

  readonly form = this.fb.group({
    evidenceType: ['InspectionReport' as EvidenceType, Validators.required],
    provider:     ['', Validators.required],
    referenceNo:  ['', Validators.required],
    status:       ['Requested' as EvidenceStatus, Validators.required],
  });

  constructor(private svc: RiskApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('submissionId') ?? '';
    this.submissionId.set(id);
    if (id) this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.svc.getEvidence(this.submissionId()).subscribe({
      next: res => { const d: any = res; this.evidence.set(d?.data ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.form.reset({ evidenceType: 'InspectionReport', status: 'Requested' });
    this.showModal.set(true);
  }

  closeModal(): void { this.showModal.set(false); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    this.svc.addEvidence(this.submissionId(), {
      evidenceType: v.evidenceType as EvidenceType,
      provider: v.provider!,
      referenceNo: v.referenceNo!,
      status: v.status as EvidenceStatus,
      resultJSON: {},
    }).subscribe({
      next: res => {
        const d: any = res;
        const added: EvidenceRef = d?.data ?? d;
        this.evidence.update(list => [added, ...list]);
        this.saving.set(false);
        this.closeModal();
        this.flash('success', 'Evidence added.');
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  // ── Inline status edit ─────────────────────────────────────────────────────

  startEdit(e: EvidenceRef): void {
    this.editingId.set(e.evidenceId);
    this.editStatus.set(e.status);
  }

  cancelEdit(): void { this.editingId.set(null); }

  updateStatus(evidenceId: string): void {
    this.updatingId.set(evidenceId);
    this.svc.updateEvidence(evidenceId, { status: this.editStatus() }).subscribe({
      next: res => {
        const d: any = res;
        const updated: EvidenceRef = d?.data ?? d;
        this.evidence.update(list =>
          list.map(e => e.evidenceId === evidenceId
            ? { ...e, status: updated?.status ?? this.editStatus() }
            : e
          )
        );
        this.updatingId.set(null);
        this.editingId.set(null);
        this.flash('success', 'Status updated.');
      },
      error: () => { this.updatingId.set(null); this.flash('danger', 'Update failed.'); },
    });
  }

  // ── Delete ─────────────────────────────────────────────────────────────────

  deleteEvidence(evidenceId: string): void {
    this.deletingId.set(evidenceId);
    this.svc.deleteEvidence(evidenceId).subscribe({
      next: () => {
        this.evidence.update(list => list.filter(e => e.evidenceId !== evidenceId));
        this.deletingId.set(null);
        this.flash('success', 'Evidence removed.');
      },
      error: () => { this.deletingId.set(null); this.flash('danger', 'Delete failed.'); },
    });
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  statusClass(s: string): string {
    return s === 'Received' ? 'bg-success' : s === 'Requested' ? 'bg-warning text-dark' : 'bg-secondary';
  }

  flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
