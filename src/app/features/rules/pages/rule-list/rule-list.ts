import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import { UWRule, RuleSeverity, RuleStatus } from '../../models/rules.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

type ModalMode = 'create' | 'edit' | null;
const SEVERITIES: RuleSeverity[] = ['Block', 'Refer', 'Load', 'Info'];

@Component({
  selector: 'app-rule-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './rule-list.html',
  styleUrl: './rule-list.css',
})
export class RuleListPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly rules        = signal<UWRule[]>([]);
  readonly loading      = signal(false);
  readonly saving       = signal(false);
  readonly modalMode    = signal<ModalMode>(null);
  readonly selected     = signal<UWRule | null>(null);
  readonly filterSev    = signal('');
  readonly filterStatus = signal('');
  readonly alertMsg     = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly severities   = SEVERITIES;

  readonly filtered = computed(() => {
    const sev = this.filterSev();
    const st  = this.filterStatus();
    return this.rules().filter(r => (!sev || r.severity === sev) && (!st || r.status === st));
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Rules & Scoring' },
    { label: 'UW Rules' },
  ];

  readonly form = this.fb.group({
    productLine: ['', Validators.required],
    severity:    ['Refer' as RuleSeverity, Validators.required],
    status:      ['Active' as RuleStatus, Validators.required],
    expression:  ['', Validators.required],
  });

  constructor(private svc: RulesApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getRules(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.rules.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.form.reset({ severity: 'Refer', status: 'Active' }); this.selected.set(null); this.modalMode.set('create'); }

  openEdit(r: UWRule): void {
    this.selected.set(r);
    this.form.patchValue({
      productLine: r.productLine, severity: r.severity, status: r.status,
      expression: JSON.stringify(r.expressionJSON),
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    let expressionJSON: Record<string, unknown> = {};
    try { expressionJSON = JSON.parse(v.expression ?? '{}'); } catch { expressionJSON = { raw: v.expression }; }
    const payload: Partial<UWRule> = {
      productLine: v.productLine!, severity: v.severity as RuleSeverity,
      status: v.status as RuleStatus, expressionJSON,
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateRule(this.selected()!.ruleId, payload)
      : this.svc.createRule(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Rule saved.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  severityClass(s: string): string {
    const map: Record<string, string> = {
      Block: 'bg-danger', Refer: 'bg-warning text-dark', Load: 'bg-info text-dark', Info: 'bg-secondary',
    };
    return map[s] ?? 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
