import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { RulesApiService } from '../../services/rules-api.service';
import { UWRule } from '../../models/rules.model';

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

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Rules', route: '/rules/list' },
    { label: 'Rule' },
  ];

  readonly productLines = ['Life', 'Health', 'PnC', 'Commercial', 'All'];
  readonly severities   = ['Block', 'Refer', 'Load', 'Info'];
  readonly statuses     = ['Active', 'Inactive'];

  readonly form = this.fb.group({
    productLine:    ['Life', Validators.required],
    severity:       ['Refer', Validators.required],
    status:         ['Active', Validators.required],
    expressionJSON: ['{}', Validators.required],
  });

  constructor(
    private svc: RulesApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.ruleId.set(id);
      this.loading.set(true);
      this.svc.getRules({ page: 0, size: 200, sort: 'ruleId', direction: 'asc' }).subscribe({
        next: (res: any) => {
          const list: UWRule[] = res?.content ?? res?.data ?? [];
          const rule = list.find(r => r.ruleId === id);
          if (rule) {
            this.form.patchValue({
              productLine:    rule.productLine,
              severity:       rule.severity,
              status:         rule.status,
              expressionJSON: JSON.stringify(rule.expressionJSON, null, 2),
            });
          }
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.flash('danger', 'Failed to load rule.'); },
      });
    }
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    let expr: Record<string, unknown> = {};
    try { expr = JSON.parse(v.expressionJSON!); } catch { expr = { raw: v.expressionJSON }; }
    const payload = {
      productLine:    v.productLine!,
      severity:       v.severity as any,
      status:         v.status as any,
      expressionJSON: expr,
    };
    this.saving.set(true);
    const req = this.ruleId()
      ? this.svc.updateRule(this.ruleId()!, payload)
      : this.svc.createRule(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/rules/list']); },
      error: () => { this.saving.set(false); this.flash('danger', 'Save failed.'); },
    });
  }

  cancel() { this.router.navigate(['/rules/list']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
