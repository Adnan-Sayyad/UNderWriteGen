import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';

interface TemplateConfig {
  id: string;
  name: string;
  type: string;
  version: string;
  active: boolean;
  updatedDate: string;
}

type ModalMode = 'create' | 'edit' | null;

const SAMPLE: TemplateConfig[] = [
  { id: '1', name: 'Life Questionnaire v2', type: 'Questionnaire', version: '2.0', active: true, updatedDate: '2024-11-01' },
  { id: '2', name: 'Commercial Risk Form',  type: 'Questionnaire', version: '1.3', active: true, updatedDate: '2024-09-15' },
  { id: '3', name: 'Quote Letter Template', type: 'Document',      version: '3.1', active: true, updatedDate: '2025-01-10' },
  { id: '4', name: 'Policy Schedule',       type: 'Document',      version: '2.5', active: true, updatedDate: '2025-02-20' },
  { id: '5', name: 'Renewal Notice',        type: 'Notification',  version: '1.0', active: false, updatedDate: '2024-07-05' },
];

const TYPES = ['Questionnaire', 'Document', 'Notification', 'Email', 'Report'];

@Component({
  selector: 'app-template-config',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './template-config.html',
  styleUrl: './template-config.css',
})
export class TemplateConfigPage {
  private readonly fb = inject(FormBuilder);

  readonly templates = signal<TemplateConfig[]>(SAMPLE);
  readonly modalMode = signal<ModalMode>(null);
  readonly selected  = signal<TemplateConfig | null>(null);
  readonly alertMsg  = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly types     = TYPES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Admin Console', route: '/admin' },
    { label: 'Template Configuration' },
  ];

  readonly form = this.fb.group({
    name:    ['', Validators.required],
    type:    ['Questionnaire', Validators.required],
    version: ['1.0', Validators.required],
    active:  [true],
  });

  constructor() {}

  openCreate(): void { this.form.reset({ active: true, type: 'Questionnaire', version: '1.0' }); this.selected.set(null); this.modalMode.set('create'); }

  openEdit(t: TemplateConfig): void {
    this.selected.set(t);
    this.form.patchValue({ name: t.name, type: t.type, version: t.version, active: t.active });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const today = new Date().toISOString().split('T')[0];
    if (this.modalMode() === 'edit' && this.selected()) {
      this.templates.update(list => list.map(t =>
        t.id === this.selected()!.id ? { ...t, ...v as any, updatedDate: today } : t
      ));
    } else {
      this.templates.update(list => [...list, { id: Date.now().toString(), ...v as any, updatedDate: today }]);
    }
    this.flash('success', 'Template saved.');
    this.closeModal();
  }

  typeClass(t: string): string {
    const map: Record<string, string> = {
      Questionnaire: 'bg-primary', Document: 'bg-success',
      Notification: 'bg-warning text-dark', Email: 'bg-info text-dark', Report: 'bg-secondary',
    };
    return map[t] ?? 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
