import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';

interface ProductConfig {
  id: string;
  name: string;
  code: string;
  category: string;
  active: boolean;
  createdDate: string;
}

type ModalMode = 'create' | 'edit' | null;

const SAMPLE: ProductConfig[] = [
  { id: '1', name: 'Term Life Insurance', code: 'LIFE-TERM', category: 'Life', active: true, createdDate: '2024-01-01' },
  { id: '2', name: 'Group Health Plan', code: 'HEALTH-GRP', category: 'Health', active: true, createdDate: '2024-02-15' },
  { id: '3', name: 'Commercial Property', code: 'PNC-COMM', category: 'PnC', active: true, createdDate: '2024-03-10' },
  { id: '4', name: 'Marine Cargo', code: 'MARINE-CARGO', category: 'Commercial', active: false, createdDate: '2024-04-01' },
];

@Component({
  selector: 'app-product-config',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './product-config.html',
  styleUrl: './product-config.css',
})
export class ProductConfigPage {
  private readonly fb = inject(FormBuilder);

  readonly products  = signal<ProductConfig[]>(SAMPLE);
  readonly modalMode = signal<ModalMode>(null);
  readonly selected  = signal<ProductConfig | null>(null);
  readonly alertMsg  = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Admin Console', route: '/admin' },
    { label: 'Product Configuration' },
  ];

  readonly form = this.fb.group({
    name:     ['', Validators.required],
    code:     ['', Validators.required],
    category: ['Life', Validators.required],
    active:   [true],
  });

  constructor() {}

  openCreate(): void { this.form.reset({ active: true, category: 'Life' }); this.selected.set(null); this.modalMode.set('create'); }

  openEdit(p: ProductConfig): void {
    this.selected.set(p);
    this.form.patchValue({ name: p.name, code: p.code, category: p.category, active: p.active });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    if (this.modalMode() === 'edit' && this.selected()) {
      this.products.update(list => list.map(p =>
        p.id === this.selected()!.id ? { ...p, ...v as any } : p
      ));
      this.flash('success', 'Product updated.');
    } else {
      const newP: ProductConfig = {
        id: Date.now().toString(), name: v.name!, code: v.code!,
        category: v.category!, active: !!v.active,
        createdDate: new Date().toISOString().split('T')[0],
      };
      this.products.update(list => [...list, newP]);
      this.flash('success', 'Product created.');
    }
    this.closeModal();
  }

  toggleActive(p: ProductConfig): void {
    this.products.update(list => list.map(x => x.id === p.id ? { ...x, active: !x.active } : x));
  }

  categoryClass(c: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark', PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[c] ?? 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
