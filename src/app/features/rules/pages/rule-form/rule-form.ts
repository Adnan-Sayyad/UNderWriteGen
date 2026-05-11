import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray, FormGroup } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { RulesApiService } from '../../services/rules-api.service';
import {
  RuleSeverity, UWStatus, ConditionOperator, FieldType,
  RuleFieldDef, RuleCondition, RuleExpression,
  SEVERITIES, UW_STATUSES, PRODUCT_LINES, ProductLine,
  FIELDS_BY_PRODUCT, OPERATORS_BY_TYPE,
  parseExpression, describeExpression,
} from '../../models/rules.model';

@Component({
  selector: 'app-rule-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './rule-form.html',
  styleUrl: './rule-form.css',
})
export class RuleFormPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly ruleId   = signal<string | null>(null);
  readonly loading  = signal(false);
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly productLines = PRODUCT_LINES;
  readonly severities   = SEVERITIES;
  readonly statuses     = UW_STATUSES;

  readonly form = this.fb.group({
    productLine: ['Life' as ProductLine, Validators.required],
    ruleName:    ['', [Validators.required, Validators.minLength(3), Validators.maxLength(120)]],
    description: [''],
    severity:    ['Refer' as RuleSeverity, Validators.required],
    status:      ['Active' as UWStatus, Validators.required],
    logic:       ['AND' as 'AND' | 'OR', Validators.required],
    conditions:  this.fb.array<FormGroup>([]),
  });

  readonly productLineSig = signal<ProductLine>('Life');
  readonly conditionsSig  = signal<RuleCondition[]>([]);

  readonly availableFields = computed<RuleFieldDef[]>(
    () => FIELDS_BY_PRODUCT[this.productLineSig()] ?? [],
  );

  readonly preview = computed(() =>
    describeExpression({ logic: this.form.controls.logic.value ?? 'AND', conditions: this.conditionsSig() })
  );

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Rules', route: '/rules/list' },
    { label: 'Rule' },
  ];

  get conditions(): FormArray<FormGroup> {
    return this.form.controls.conditions as FormArray<FormGroup>;
  }

  constructor(
    private svc: RulesApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.form.controls.productLine.valueChanges.subscribe(p => {
      if (p) this.productLineSig.set(p as ProductLine);
    });

    this.form.controls.conditions.valueChanges.subscribe(() => {
      this.conditionsSig.set(this.conditions.controls.map(c => c.value as RuleCondition));
    });

    this.form.controls.logic.valueChanges.subscribe(() => {
      // Recompute preview by re-emitting conditions snapshot.
      this.conditionsSig.set([...this.conditionsSig()]);
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.addCondition();
      return;
    }

    this.ruleId.set(id);
    this.loading.set(true);
    this.svc.getRuleById(id).subscribe({
      next: rule => {
        this.loading.set(false);
        if (!rule) { this.flash('danger', 'Rule not found.'); return; }
        const expr = parseExpression(rule.expressionJSON);
        this.form.patchValue({
          productLine: (rule.productLine as ProductLine) ?? 'Life',
          ruleName:    rule.ruleName ?? '',
          description: rule.description ?? '',
          severity:    rule.severity,
          status:      rule.status,
          logic:       expr.logic,
        });
        this.productLineSig.set((rule.productLine as ProductLine) ?? 'Life');
        this.conditions.clear();
        expr.conditions.forEach(c => this.conditions.push(this.buildConditionGroup(c)));
        if (!this.conditions.length) this.addCondition();
      },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load rule.'); },
    });
  }

  // ─── Condition row helpers ─────────────────────────────────────────────────

  buildConditionGroup(initial?: Partial<RuleCondition>): FormGroup {
    const fieldKey = initial?.field ?? this.availableFields()[0]?.key ?? '';
    const fieldDef = this.findFieldDef(fieldKey);
    const type     = (initial?.type ?? fieldDef?.type ?? 'number') as FieldType;
    const opts     = OPERATORS_BY_TYPE[type];
    const op       = (initial?.operator ?? opts[0]?.value ?? 'eq') as ConditionOperator;

    return this.fb.group({
      field:      [fieldKey, Validators.required],
      fieldLabel: [initial?.fieldLabel ?? fieldDef?.label ?? fieldKey],
      type:       [type, Validators.required],
      unit:       [initial?.unit ?? fieldDef?.unit ?? ''],
      operator:   [op, Validators.required],
      value:      [this.coerceInitialValue(initial?.value, type), Validators.required],
      value2:     [initial?.value2 ?? null],
      options:    [fieldDef?.options ?? []],
    });
  }

  addCondition(): void {
    this.conditions.push(this.buildConditionGroup());
  }

  removeCondition(i: number): void {
    if (this.conditions.length > 1) this.conditions.removeAt(i);
    else this.flash('danger', 'A rule needs at least one condition.');
  }

  onFieldChange(i: number, fieldKey: string): void {
    const def = this.findFieldDef(fieldKey);
    if (!def) return;
    const row = this.conditions.at(i);
    const ops = OPERATORS_BY_TYPE[def.type];
    row.patchValue({
      field:      def.key,
      fieldLabel: def.label,
      type:       def.type,
      unit:       def.unit ?? '',
      operator:   ops[0]?.value ?? 'eq',
      value:      this.defaultValueFor(def.type),
      value2:     null,
      options:    def.options ?? [],
    });
  }

  operatorsFor(type: FieldType) {
    return OPERATORS_BY_TYPE[type] ?? [];
  }

  isBetween(row: FormGroup): boolean {
    return row.controls['operator']?.value === 'between';
  }

  isMultiSelect(row: FormGroup): boolean {
    return row.controls['operator']?.value === 'in';
  }

  fieldHelp(row: FormGroup): string | null {
    const def = this.findFieldDef(row.controls['field'].value);
    return def?.description ?? null;
  }

  toggleMulti(row: FormGroup, optionValue: string, checked: boolean): void {
    const ctrl = row.controls['value'];
    const current: string[] = Array.isArray(ctrl.value) ? [...ctrl.value] : [];
    const next = checked
      ? Array.from(new Set([...current, optionValue]))
      : current.filter(v => v !== optionValue);
    ctrl.setValue(next);
  }

  isMultiChecked(row: FormGroup, optionValue: string): boolean {
    const v = row.controls['value'].value;
    return Array.isArray(v) && v.includes(optionValue);
  }

  // ─── Save ──────────────────────────────────────────────────────────────────

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.flash('danger', 'Please fill the rule details and at least one condition.');
      return;
    }

    const expr: RuleExpression = {
      logic: this.form.controls.logic.value ?? 'AND',
      conditions: this.conditions.controls.map(c => {
        const v = c.value as any;
        return {
          field:      v.field,
          fieldLabel: v.fieldLabel,
          type:       v.type,
          operator:   v.operator,
          value:      this.normalizeValue(v.value, v.type, v.operator),
          ...(v.operator === 'between' ? { value2: this.toNumber(v.value2) } : {}),
          ...(v.unit ? { unit: v.unit } : {}),
        } as RuleCondition;
      }),
    };

    const v = this.form.value;
    const payload = {
      productLine:    v.productLine!,
      ruleName:       v.ruleName?.trim() || null,
      description:    v.description?.trim() || null,
      expressionJSON: JSON.stringify(expr),
      severity:       v.severity as RuleSeverity,
      status:         v.status as UWStatus,
    };

    this.saving.set(true);
    const call$ = this.ruleId()
      ? this.svc.updateRule(this.ruleId()!, payload)
      : this.svc.createRule(payload);

    call$.subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/rules/list']); },
      error: err => {
        this.saving.set(false);
        this.flash('danger', err?.error?.message ?? 'Save failed.');
      },
    });
  }

  cancel(): void { this.router.navigate(['/rules/list']); }

  // ─── Internals ─────────────────────────────────────────────────────────────

  private findFieldDef(key: string): RuleFieldDef | undefined {
    return this.availableFields().find(f => f.key === key);
  }

  private defaultValueFor(type: FieldType): any {
    switch (type) {
      case 'boolean': return true;
      case 'number':  return 0;
      default:        return '';
    }
  }

  private coerceInitialValue(v: unknown, type: FieldType): any {
    if (v === undefined || v === null) return this.defaultValueFor(type);
    if (type === 'number' && typeof v === 'string') {
      const n = Number(v); return isNaN(n) ? 0 : n;
    }
    if (type === 'boolean') return !!v;
    return v;
  }

  private normalizeValue(v: any, type: FieldType, op: ConditionOperator): any {
    if (op === 'in') return Array.isArray(v) ? v : [];
    if (type === 'number')  return this.toNumber(v);
    if (type === 'boolean') return v === true || v === 'true';
    return v ?? '';
  }

  private toNumber(v: any): number {
    const n = typeof v === 'number' ? v : Number(v);
    return isNaN(n) ? 0 : n;
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
