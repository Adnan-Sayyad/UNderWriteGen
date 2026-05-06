import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ComplianceApiService } from '../../services/compliance-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import {
  AuthorityBreach, BreachType, BreachStatus,
  CreateBreachPayload, UpdateBreachStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'detail' | 'status' | null;
const BREACH_TYPES: BreachType[]   = ['Authority', 'RuleOverride', 'PricingTolerance'];
const BREACH_STATUSES: BreachStatus[] = ['Pending', 'Approved', 'Rejected'];

@Component({
  selector: 'app-authority-breaches',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './authority-breaches.html',
  styleUrl: './authority-breaches.css',
})
export class AuthorityBreachesPage implements OnInit {
  readonly breachTypes   = BREACH_TYPES;
  readonly breachStatuses = BREACH_STATUSES;
  readonly breaches      = signal<AuthorityBreach[]>([]);
  readonly loading       = signal(false);
  readonly saving        = signal(false);
  readonly modalMode     = signal<ModalMode>(null);
  readonly selected      = signal<AuthorityBreach | null>(null);
  readonly filterType    = signal('');
  readonly filterStatus  = signal('');
  readonly alertMsg      = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly filtered = computed(() => {
    const t = this.filterType();
    const s = this.filterStatus();
    return this.breaches().filter(b =>
      (!t || b.breachType === t) && (!s || b.status === s)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Authority Breaches' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    breachType:   ['Authority' as BreachType, Validators.required],
    description:  ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly statusForm = this.fb.group({
    status:     ['Pending' as BreachStatus, Validators.required],
    approvedBy: [''],
  });

  constructor(
    private svc: ComplianceApiService,
    readonly auth: AuthService,
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getBreaches().subscribe({
      next: data => { this.breaches.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  openCreate(): void { this.createForm.reset({ breachType: 'Authority' }); this.modalMode.set('create'); }
  openDetail(b: AuthorityBreach): void { this.selected.set(b); this.modalMode.set('detail'); }
  openStatus(b: AuthorityBreach): void {
    this.selected.set(b);
    this.statusForm.patchValue({ status: b.status, approvedBy: b.approvedBy ?? '' });
    this.modalMode.set('status');
  }
  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.value;
    const payload: CreateBreachPayload = {
      submissionId: v.submissionId!,
      breachType:   v.breachType as BreachType,
      description:  v.description!,
    };
    this.svc.createBreach(payload).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Authority breach logged.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Create failed.'); },
    });
  }

  saveStatus(): void {
    if (!this.selected()) return;
    this.saving.set(true);
    const v = this.statusForm.value;
    const payload: UpdateBreachStatusPayload = {
      status:      v.status as BreachStatus,
      approvedBy:  v.approvedBy || undefined,
      approvedDate: v.status !== 'Pending' ? new Date().toISOString() : undefined,
    };
    this.svc.updateBreachStatus(this.selected()!.breachId, payload).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Breach status updated.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Update failed.'); },
    });
  }

  breachTypeClass(t: string): string {
    return t === 'Authority' ? 'bg-danger' : t === 'RuleOverride' ? 'bg-warning text-dark' : 'bg-info text-dark';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
