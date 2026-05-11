import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PricingApiService } from '../../services/pricing-api.service';
import { PricingParam, CreatePricingParamRequest, UpdatePricingParamRequest } from '../../models/pricing.model';
import { HttpErrorResponse } from '@angular/common/http';

type ModalMode = 'create' | 'edit' | 'confirmDelete' | null;

const PRODUCT_LINES = ['Life', 'Health', 'PnC', 'Commercial', 'Global'];

/** Excludes "Global" — that's an internal catch-all, not a real product line users should create params for */
const CREATE_PRODUCT_LINES = ['Life', 'Health', 'PnC', 'Commercial'];

const PARAM_LABELS: Record<string, string> = {
  BaseRate:                 'Base Rate',
  RiskLoading_Medium:       'Risk Loading — Medium Band',
  RiskLoading_High:         'Risk Loading — High Band',
  OccupationLoad_Mining:    'Occupation Loading (Mining)',
  OccupationLoad_General:   'Occupation Loading (General)',
  OccupationLoad_Construction: 'Occupation Loading (Construction)',
  OccupationLoad_IT:        'Occupation Loading (IT)',
  OccupationLoad_Banking:   'Occupation Loading (Banking)',
  OccupationLoad_Agriculture: 'Occupation Loading (Agriculture)',
  OccupationLoad_Transport: 'Occupation Loading (Transport)',
  OccupationLoad_Education: 'Occupation Loading (Education)',
  TenureDiscount_12m:       'Tenure Discount — 12 months',
  TenureDiscount_24m:       'Tenure Discount — 24 months',
  TenureDiscount_36m:       'Tenure Discount — 36 months',
  LoyaltyDiscount:          'Loyalty Discount (Renewal)',
  AgentDiscount_Preferred:  'Preferred Agent Discount',
  GstRate:                  'GST Rate',
  MinimumPremium:           'Minimum Premium (Floor)',
  QuoteValidityDays:        'Quote Validity Period',
};

const PRODUCT_LINE_LABELS: Record<string, string> = {
  Life:       'Life',
  Health:     'Health',
  PnC:        'Property & Casualty',
  Commercial: 'Commercial',
  Global:     'Global (All Lines)',
};

@Component({
  selector: 'app-pricing-params',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule, PageHeader, EmptyState],
  // imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './pricing-params.html',
  styleUrl: './pricing-params.css',
})
export class PricingParamsPage implements OnInit {
  readonly params       = signal<PricingParam[]>([]);
  readonly loading      = signal(false);
  readonly saving       = signal(false);
  readonly modalMode    = signal<ModalMode>(null);
  readonly selected     = signal<PricingParam | null>(null);
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  // Filters
  readonly filterLine   = signal('');
  readonly filterDate   = signal('');
  readonly filterActive = signal<boolean | ''>('');

  readonly PRODUCT_LINES        = PRODUCT_LINES;
  readonly CREATE_PRODUCT_LINES = CREATE_PRODUCT_LINES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Pricing Console', route: '/pricing' },
    { label: 'Pricing Parameters' },
  ];

  private readonly fb = inject(FormBuilder);

  // Create form — all fields required
  readonly createForm = this.fb.group({
    productLine:   ['', [Validators.required, Validators.maxLength(50)]],
    paramName:     ['', [Validators.required, Validators.maxLength(100)]],
    value:         [0,  [Validators.required, Validators.min(0)]],
    description:   ['', [Validators.required, Validators.maxLength(250)]],
    effectiveFrom: ['', Validators.required],
    effectiveTo:   [''],   // optional
  });

  // Edit form — only value + description are editable (backend constraint)
  readonly editForm = this.fb.group({
    value:       [0,  [Validators.required, Validators.min(0)]],
    description: ['', [Validators.required, Validators.maxLength(250)]],
  });

  readonly filtered = computed(() => {
    let list = this.params();
    const line   = this.filterLine();
    const active = this.filterActive();
    if (line)        list = list.filter(p => p.productLine === line);
    if (active !== '') list = list.filter(p => p.isActive === (active as boolean));
    return list;
  });

  constructor(private svc: PricingApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    const date = this.filterDate();
    this.loading.set(true);

    const req$ = date
      ? this.svc.getParamsByEffectiveDate(date)
      : this.svc.getParams();

    req$.subscribe({
      next: params => {
        // Normalise: backend may return array or wrapped response
        const list = Array.isArray(params) ? params : (params as any)?.data ?? [];
        this.params.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  applyFilters(): void { this.load(); }

  clearFilters(): void {
    this.filterLine.set('');
    this.filterDate.set('');
    this.filterActive.set('');
    this.load();
  }

  openCreate(): void {
    this.createForm.reset({ value: 0 });
    this.selected.set(null);
    this.modalMode.set('create');
  }

  openEdit(p: PricingParam): void {
    this.selected.set(p);
    this.editForm.patchValue({ value: p.value, description: p.description });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  openDelete(p: PricingParam): void {
    this.selected.set(p);
    this.modalMode.set('confirmDelete');
  }

  confirmDelete(): void {
    const p = this.selected();
    if (!p) return;
    this.saving.set(true);
    this.svc.deleteParam(p.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', `Parameter "${p.paramName}" deleted.`);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to delete parameter.');
      },
    });
  }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    const v = this.createForm.value;
    const payload: CreatePricingParamRequest = {
      productLine:   v.productLine!,
      paramName:     v.paramName!,
      value:         v.value!,
      description:   v.description!,
      effectiveFrom: v.effectiveFrom!,
      effectiveTo:   v.effectiveTo || null,
    };
    this.saving.set(true);
    this.svc.createParam(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Pricing parameter created successfully.');
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to create parameter.');
      },
    });
  }

  saveEdit(): void {
    if (this.editForm.invalid) { this.editForm.markAllAsTouched(); return; }
    const v = this.editForm.value;
    const payload: UpdatePricingParamRequest = {
      value:       v.value!,
      description: v.description!,
    };
    this.saving.set(true);
    this.svc.updateParam(this.selected()!.id, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Pricing parameter updated successfully.');
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to update parameter.');
      },
    });
  }

  /** Human-readable parameter name */
  paramLabel(name: string): string {
    return PARAM_LABELS[name]
      ?? name.replace(/_/g, ' — ').replace(/([A-Z])/g, ' $1').trim();
  }

  /** Human-readable product line name */
  productLineLabel(line: string): string {
    return PRODUCT_LINE_LABELS[line] ?? line;
  }

  /** Product line badge CSS class */
  productLineClass(line: string): string {
    const m: Record<string, string> = {
      Life:       'bg-info-subtle text-info-emphasis border border-info-subtle',
      Health:     'bg-success-subtle text-success-emphasis border border-success-subtle',
      PnC:        'bg-warning-subtle text-warning-emphasis border border-warning-subtle',
      Commercial: 'bg-primary-subtle text-primary-emphasis border border-primary-subtle',
      Global:     'bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle',
    };
    return m[line] ?? 'bg-secondary';
  }

  /** Human-readable value — shows % for rates, ₹ for money, 'days' for validity */
  formatParamValue(name: string, value: number): string {
    if (name === 'MinimumPremium') return `₹${value.toLocaleString('en-IN')}`;
    if (name === 'QuoteValidityDays') return `${value} days`;
    if (value > 0 && value < 1)    return `${(value * 100).toFixed(1)}%`;
    return value.toString();
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
