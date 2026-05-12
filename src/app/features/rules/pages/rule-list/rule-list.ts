import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pager } from '../../components/pager/pager';
import { RulesApiService } from '../../services/rules-api.service';
import {
  UWRule, RuleSeverity, UWStatus, EvaluateRulesResponse,
  SEVERITIES, UW_STATUSES, PRODUCT_LINES, DEFAULT_PAGE_SIZE,
  parseExpression, describeExpression,
} from '../../models/rules.model';

@Component({
  selector: 'app-rule-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState, Pager],
  templateUrl: './rule-list.html',
  styleUrl: './rule-list.css',
})
export class RuleListPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly rules        = signal<UWRule[]>([]);
  readonly loading      = signal(false);
  readonly toggling     = signal<string | null>(null);
  readonly deleting     = signal<string | null>(null);

  // Filters (sent to backend)
  readonly filterSev    = signal<RuleSeverity | ''>('');
  readonly filterStatus = signal<UWStatus | ''>('');
  readonly filterProd   = signal('');

  // Pagination state
  readonly page          = signal(0);
  readonly size          = signal(DEFAULT_PAGE_SIZE);
  readonly totalElements = signal(0);
  readonly totalPages    = signal(0);

  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly evaluateOpen   = signal(false);
  readonly evaluating     = signal(false);
  readonly evaluation     = signal<EvaluateRulesResponse | null>(null);

  readonly severities   = SEVERITIES;
  readonly statuses     = UW_STATUSES;
  readonly productLines = PRODUCT_LINES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Rules & Scoring' },
    { label: 'UW Rules' },
  ];

  readonly evalForm = this.fb.group({
    submissionId: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor(private svc: RulesApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getRules(this.page(), this.size(), {
      productLine: this.filterProd(),
      severity:    this.filterSev(),
      status:      this.filterStatus(),
    }).subscribe({
      next: r => {
        this.rules.set(r.content);
        this.totalElements.set(r.totalElements);
        this.totalPages.set(r.totalPages);
        // Backend may have clamped page if out of range; reflect it
        this.page.set(r.page);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load rules.'); },
    });
  }

  /** Filters change → reset to first page and reload. */
  applyFilter<T extends string>(setter: (v: T) => void, value: T): void {
    setter(value);
    this.page.set(0);
    this.load();
  }

  setSev(v: string)    { this.applyFilter(x => this.filterSev.set(x as RuleSeverity | ''), v as RuleSeverity | ''); }
  setStatus(v: string) { this.applyFilter(x => this.filterStatus.set(x as UWStatus | ''), v as UWStatus | ''); }
  setProd(v: string)   { this.applyFilter(x => this.filterProd.set(x), v); }

  onPageChange(p: number) { this.page.set(p); this.load(); }
  onSizeChange(s: number) { this.size.set(s); this.page.set(0); this.load(); }

  toggleStatus(r: UWRule): void {
    const next: UWStatus = r.status === 'Active' ? 'Inactive' : 'Active';
    this.toggling.set(r.uwRuleID);
    this.svc.updateRuleStatus(r.uwRuleID, next).subscribe({
      next: updated => {
        this.toggling.set(null);
        this.rules.update(list => list.map(x => x.uwRuleID === r.uwRuleID ? updated : x));
        this.flash('success', `Rule marked ${next}.`);
      },
      error: () => { this.toggling.set(null); this.flash('danger', 'Status update failed.'); },
    });
  }

  remove(r: UWRule): void {
    if (!confirm(`Delete rule for ${r.productLine}? This cannot be undone.`)) return;
    this.deleting.set(r.uwRuleID);
    this.svc.deleteRule(r.uwRuleID).subscribe({
      next: () => {
        this.deleting.set(null);
        this.flash('success', 'Rule deleted.');
        // Reload — may need to step back a page if we deleted the last item on this page.
        if (this.rules().length === 1 && this.page() > 0) this.page.update(p => p - 1);
        this.load();
      },
      error: () => { this.deleting.set(null); this.flash('danger', 'Delete failed.'); },
    });
  }

  openEvaluate(): void {
    this.evaluation.set(null);
    this.evalForm.reset({ submissionId: '' });
    this.evaluateOpen.set(true);
  }

  closeEvaluate(): void { this.evaluateOpen.set(false); }

  runEvaluate(): void {
    if (this.evalForm.invalid) { this.evalForm.markAllAsTouched(); return; }
    this.evaluating.set(true);
    this.svc.evaluateRules((this.evalForm.value.submissionId ?? '').trim()).subscribe({
      next: res => { this.evaluation.set(res); this.evaluating.set(false); },
      error: () => { this.evaluating.set(false); this.flash('danger', 'Evaluation failed.'); },
    });
  }

  severityClass(s: RuleSeverity): string {
    const map: Record<RuleSeverity, string> = {
      Block: 'bg-danger', Refer: 'bg-warning text-dark',
      Load:  'bg-info text-dark', Info: 'bg-secondary',
    };
    return map[s] ?? 'bg-secondary';
  }

  exprPreview(r: UWRule): string {
    const expr = parseExpression(r.expressionJSON);
    if (!expr.conditions.length) return '—';
    const text = describeExpression(expr);
    return text.length > 100 ? `${text.slice(0, 100)}…` : text;
  }

  ruleTitle(r: UWRule): string {
    return r.ruleName?.trim() || `Rule ${r.uwRuleID.slice(0, 8)}…`;
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
