import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';

interface SlaConfig {
  id: string;
  process: string;
  targetDays: number;
  warningDays: number;
  productLine: string;
  active: boolean;
}

type ModalMode = 'create' | 'edit' | null;

const SAMPLE: SlaConfig[] = [
  { id: '1', process: 'Intake Review',     targetDays: 2,  warningDays: 1,  productLine: 'All',        active: true },
  { id: '2', process: 'UW Decision',       targetDays: 5,  warningDays: 4,  productLine: 'Life',       active: true },
  { id: '3', process: 'Quote Issuance',    targetDays: 3,  warningDays: 2,  productLine: 'All',        active: true },
  { id: '4', process: 'Policy Binding',    targetDays: 2,  warningDays: 1,  productLine: 'Commercial', active: true },
  { id: '5', process: 'Renewal Offer',     targetDays: 30, warningDays: 35, productLine: 'All',        active: true },
];

@Component({
  selector: 'app-sla-config',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './sla-config.html',
  styleUrl: './sla-config.css',
})
export class SlaConfigPage {
  private readonly fb = inject(FormBuilder);

  readonly slas      = signal<SlaConfig[]>(SAMPLE);
  readonly modalMode = signal<ModalMode>(null);
  readonly selected  = signal<SlaConfig | null>(null);
  readonly alertMsg  = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Admin Console', route: '/admin' },
    { label: 'SLA Configuration' },
  ];

  readonly form = this.fb.group({
    process:     ['', Validators.required],
    targetDays:  [1, [Validators.required, Validators.min(1)]],
    warningDays: [1, [Validators.required, Validators.min(1)]],
    productLine: ['All', Validators.required],
    active:      [true],
  });

  constructor() {}

  openCreate(): void { this.form.reset({ active: true, targetDays: 3, warningDays: 2, productLine: 'All' }); this.selected.set(null); this.modalMode.set('create'); }

  openEdit(s: SlaConfig): void {
    this.selected.set(s);
    this.form.patchValue({ process: s.process, targetDays: s.targetDays, warningDays: s.warningDays, productLine: s.productLine, active: s.active });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    if (this.modalMode() === 'edit' && this.selected()) {
      this.slas.update(list => list.map(s => s.id === this.selected()!.id ? { ...s, ...v as any } : s));
    } else {
      this.slas.update(list => [...list, { id: Date.now().toString(), ...v as any }]);
    }
    this.flash('success', 'SLA saved.');
    this.closeModal();
  }

  toggleActive(s: SlaConfig): void {
    this.slas.update(list => list.map(x => x.id === s.id ? { ...x, active: !x.active } : x));
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
