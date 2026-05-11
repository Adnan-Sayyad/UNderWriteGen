import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PricingApiService } from '../../services/pricing-api.service';
import { PricingParam, ParamStatus } from '../../models/pricing.model';

type ModalMode = 'create' | 'edit' | null;

@Component({
  selector: 'app-pricing-params',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './pricing-params.html',
  styleUrl: './pricing-params.css',
})
export class PricingParamsPage implements OnInit {
  readonly params    = signal<PricingParam[]>([]);
  readonly loading   = signal(false);
  readonly saving    = signal(false);
  readonly modalMode = signal<ModalMode>(null);
  readonly selected  = signal<PricingParam | null>(null);
  readonly alertMsg  = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Pricing Console', route: '/pricing' },
    { label: 'Pricing Parameters' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    productLine:   ['', Validators.required],
    factorName:    ['', Validators.required],
    effectiveFrom: ['', Validators.required],
    effectiveTo:   ['', Validators.required],
    status:        ['Active' as ParamStatus, Validators.required],
  });

  constructor(private svc: PricingApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getParams().subscribe({
      next: res => {
        const d: any = res;
        this.params.set(d?.data ?? d ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.form.reset({ status: 'Active' }); this.selected.set(null); this.modalMode.set('create'); }

  openEdit(p: PricingParam): void {
    this.selected.set(p);
    this.form.patchValue({
      productLine: p.productLine, factorName: p.factorName,
      effectiveFrom: p.effectiveFrom?.split('T')[0],
      effectiveTo: p.effectiveTo?.split('T')[0],
      status: p.status,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload: Partial<PricingParam> = {
      productLine: v.productLine!, factorName: v.factorName!,
      effectiveFrom: v.effectiveFrom!, effectiveTo: v.effectiveTo!,
      status: v.status as ParamStatus, factorTableJSON: {},
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateParam(this.selected()!.paramId, payload)
      : this.svc.saveParam(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Parameter saved.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  statusClass(s: string): string { return s === 'Active' ? 'bg-success' : 'bg-secondary'; }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
