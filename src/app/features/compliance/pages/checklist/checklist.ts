import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { ComplianceApiService } from '../../services/compliance-api.service';
import {
  ComplianceChecklist, ChecklistStatus,
  CreateChecklistPayload, UpdateChecklistStatusPayload,
} from '../../models/compliance.model';

type ModalMode = 'create' | 'view' | 'status' | null;
const STATUSES: ChecklistStatus[] = ['Pending', 'InProgress', 'Completed'];

@Component({
  selector: 'app-checklist',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './checklist.html',
  styleUrl: './checklist.css',
})
export class ChecklistPage implements OnInit {
  readonly statuses    = STATUSES;
  readonly checklists  = signal<ComplianceChecklist[]>([]);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly modalMode   = signal<ModalMode>(null);
  readonly selected    = signal<ComplianceChecklist | null>(null);
  readonly filterStatus = signal('');
  readonly alertMsg    = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return s ? this.checklists().filter(c => c.status === s) : this.checklists();
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Checklists' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly createForm = this.fb.group({
    submissionId: ['', Validators.required],
    completedBy:  [''],
    status:       ['Pending' as ChecklistStatus, Validators.required],
  });

  readonly statusForm = this.fb.group({
    status: ['Pending' as ChecklistStatus, Validators.required],
  });

  /* checklist items managed locally before serialising to JSON */
  readonly items = signal<{ item: string; checked: boolean }[]>([]);
  newItemText = '';

  constructor(private svc: ComplianceApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getChecklists().subscribe({
      next: data => { this.checklists.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.createForm.reset({ status: 'Pending' });
    this.items.set([]);
    this.modalMode.set('create');
  }

  openView(c: ComplianceChecklist): void {
    this.selected.set(c);
    try { this.items.set(JSON.parse(c.itemsJson)); } catch { this.items.set([]); }
    this.modalMode.set('view');
  }

  openStatus(c: ComplianceChecklist): void {
    this.selected.set(c);
    this.statusForm.patchValue({ status: c.status });
    this.modalMode.set('status');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  addItem(): void {
    const t = this.newItemText.trim();
    if (!t) return;
    this.items.update(list => [...list, { item: t, checked: false }]);
    this.newItemText = '';
  }

  toggleItem(idx: number): void {
    this.items.update(list =>
      list.map((it, i) => i === idx ? { ...it, checked: !it.checked } : it)
    );
  }

  removeItem(idx: number): void {
    this.items.update(list => list.filter((_, i) => i !== idx));
  }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.value;
    const payload: CreateChecklistPayload = {
      submissionId: v.submissionId!,
      itemsJson:    JSON.stringify(this.items()),
      completedBy:  v.completedBy || undefined,
      status:       v.status as ChecklistStatus,
    };
    this.svc.createChecklist(payload).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Checklist created.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Create failed.'); },
    });
  }

  saveStatus(): void {
    if (!this.selected()) return;
    this.saving.set(true);
    const payload: UpdateChecklistStatusPayload = { status: this.statusForm.value.status as ChecklistStatus };
    this.svc.updateChecklistStatus(this.selected()!.checklistId, payload).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Status updated.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Update failed.'); },
    });
  }

  progressOf(c: ComplianceChecklist): { done: number; total: number; pct: number } {
    try {
      const items: { checked: boolean }[] = JSON.parse(c.itemsJson);
      const done  = items.filter(i => i.checked).length;
      return { done, total: items.length, pct: items.length ? Math.round((done / items.length) * 100) : 0 };
    } catch { return { done: 0, total: 0, pct: 0 }; }
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
