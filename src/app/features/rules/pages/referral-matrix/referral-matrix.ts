import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import {
  ReferralMatrix, CriteriaType, Authority, UWStatus, ConditionOperator,
  AUTHORITIES, CRITERIA_TYPES, UW_STATUSES, PRODUCT_LINES, RISK_BANDS,
  OPERATORS_BY_TYPE, operatorSymbol,
} from '../../models/rules.model';

type ModalMode = 'create' | 'edit' | null;

@Component({
  selector: 'app-referral-matrix',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './referral-matrix.html',
  styleUrl: './referral-matrix.css',
})
export class ReferralMatrixPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly matrices    = signal<ReferralMatrix[]>([]);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly deleting    = signal<string | null>(null);
  readonly modalMode   = signal<ModalMode>(null);
  readonly selected    = signal<ReferralMatrix | null>(null);
  readonly filterProd  = signal('');
  readonly filterAuth  = signal<Authority | ''>('');
  readonly alertMsg    = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly productLines  = PRODUCT_LINES;
  readonly authorities   = AUTHORITIES;
  readonly criteriaTypes = CRITERIA_TYPES;
  readonly statuses      = UW_STATUSES;
  readonly riskBands     = RISK_BANDS;

  // class options used when CriteriaJSON === 'Class'
  readonly classOptions  = ['A', 'B', 'C', 'D', 'E'];

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Rules', route: '/rules/list' },
    { label: 'Referral Matrix' },
  ];

  readonly filtered = computed(() => {
    const p = this.filterProd();
    const a = this.filterAuth();
    return this.matrices().filter(m =>
      (!p || m.productLine === p) && (!a || m.requiredAuthority === a));
  });

  readonly form = this.fb.group({
    productLine:       ['Life', Validators.required],
    criteriaJSON:      ['SumInsured' as CriteriaType, Validators.required],
    operator:          ['gt' as ConditionOperator, Validators.required],
    threshold:         ['', Validators.required],
    requiredAuthority: ['UW1' as Authority, Validators.required],
    status:            ['Active' as UWStatus, Validators.required],
  });

  // Watch criteria changes so the value/operator stay sensible
  readonly criteriaSig = signal<CriteriaType>('SumInsured');

  // Operator options shown in the form (depend on criteria type)
  readonly operatorOptions = computed(() => {
    const c = this.criteriaSig();
    if (c === 'SumInsured') return OPERATORS_BY_TYPE.number;
    if (c === 'RiskBand')   return OPERATORS_BY_TYPE.enum;
    return OPERATORS_BY_TYPE.string;
  });

  constructor(private svc: RulesApiService) {
    this.form.controls.criteriaJSON.valueChanges.subscribe(c => {
      if (!c) return;
      this.criteriaSig.set(c);
      // Re-default operator and threshold to sensible values for the new criteria
      const opts = this.operatorOptions();
      this.form.patchValue({
        operator:  opts.some(o => o.value === this.form.controls.operator.value)
                     ? this.form.controls.operator.value
                     : opts[0]?.value ?? 'eq',
        threshold: c === 'SumInsured' ? '1000000' : c === 'RiskBand' ? 'High' : 'A',
      });
    });
  }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getMatrices().subscribe({
      next: m => { this.matrices.set(m); this.loading.set(false); },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load referral matrix.'); },
    });
  }

  openCreate(): void {
    this.selected.set(null);
    this.criteriaSig.set('SumInsured');
    this.form.reset({
      productLine: 'Life', criteriaJSON: 'SumInsured',
      operator: 'gt', threshold: '1000000',
      requiredAuthority: 'UW1', status: 'Active',
    });
    this.modalMode.set('create');
  }

  openEdit(m: ReferralMatrix): void {
    this.selected.set(m);
    this.criteriaSig.set(m.criteriaJSON);
    this.form.patchValue({
      productLine:       m.productLine,
      criteriaJSON:      m.criteriaJSON,
      operator:          (m.operator ?? 'eq') as ConditionOperator,
      threshold:         m.threshold ?? '',
      requiredAuthority: m.requiredAuthority,
      status:            m.status,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const payload = {
      productLine:       v.productLine!,
      criteriaJSON:      v.criteriaJSON as CriteriaType,
      operator:          (v.operator ?? null) as ConditionOperator | null,
      threshold:         (v.threshold ?? '').toString().trim() || null,
      requiredAuthority: v.requiredAuthority as Authority,
      status:            v.status as UWStatus,
    };
    this.saving.set(true);
    const call$ = this.modalMode() === 'edit'
      ? this.svc.updateMatrix(this.selected()!.referralMatrixID, payload)
      : this.svc.createMatrix(payload);

    call$.subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Matrix entry saved.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  remove(m: ReferralMatrix): void {
    if (!confirm(`Delete matrix entry for ${m.productLine} / ${m.criteriaJSON}?`)) return;
    this.deleting.set(m.referralMatrixID);
    this.svc.deleteMatrix(m.referralMatrixID).subscribe({
      next: () => {
        this.deleting.set(null);
        this.matrices.update(list => list.filter(x => x.referralMatrixID !== m.referralMatrixID));
        this.flash('success', 'Matrix entry deleted.');
      },
      error: () => { this.deleting.set(null); this.flash('danger', 'Delete failed.'); },
    });
  }

  toggleStatus(m: ReferralMatrix): void {
    const next: UWStatus = m.status === 'Active' ? 'Inactive' : 'Active';
    this.svc.updateMatrixStatus(m.referralMatrixID, next).subscribe({
      next: updated => {
        this.matrices.update(list => list.map(x => x.referralMatrixID === m.referralMatrixID ? updated : x));
        this.flash('success', `Matrix entry ${next}.`);
      },
      error: () => this.flash('danger', 'Status change failed.'),
    });
  }

  // ─── Display helpers ───────────────────────────────────────────────────────

  formatThreshold(m: ReferralMatrix): string {
    if (!m.threshold) return '—';
    if (m.criteriaJSON === 'SumInsured') {
      const n = Number(m.threshold);
      return isNaN(n) ? m.threshold : `$${n.toLocaleString()}`;
    }
    return m.threshold;
  }

  ruleSentence(m: ReferralMatrix): string {
    if (!m.operator || !m.threshold) return `${m.criteriaJSON} → ${m.requiredAuthority}`;
    return `When ${m.criteriaJSON} ${operatorSymbol(m.operator)} ${this.formatThreshold(m)} → refer to ${m.requiredAuthority}`;
  }

  previewSentence(): string {
    const v = this.form.value;
    const criteria = v.criteriaJSON ?? 'SumInsured';
    const op = operatorSymbol(v.operator as ConditionOperator);
    const thr = this.formatThresholdValue(criteria as CriteriaType, v.threshold ?? '');
    return `When ${criteria} ${op} ${thr} on ${v.productLine}, refer to ${v.requiredAuthority}.`;
  }

  formatThresholdValue(criteria: CriteriaType, raw: string): string {
    if (!raw) return '?';
    if (criteria === 'SumInsured') {
      const n = Number(raw); return isNaN(n) ? raw : `$${n.toLocaleString()}`;
    }
    return raw;
  }

  authorityClass(a: Authority): string {
    const map: Record<Authority, string> = {
      UW1: 'bg-primary', UW2: 'bg-info text-dark',
      UWManager: 'bg-warning text-dark', Committee: 'bg-danger',
    };
    return map[a] ?? 'bg-secondary';
  }

  criteriaIcon(c: CriteriaType): string {
    const map: Record<CriteriaType, string> = {
      SumInsured: 'bi-currency-dollar', Class: 'bi-tags', RiskBand: 'bi-bar-chart-steps',
    };
    return map[c] ?? 'bi-grid';
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
