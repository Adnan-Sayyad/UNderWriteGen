import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PartyApiService } from '../../services/party-api.service';
import { Agent, AgentStatus } from '../../models/party.model';
import {
  noDigitsValidator,
  smartContactInfoValidator,
  extractApiErrors,
} from '../../../../shared/validators/custom-validators';

type ModalMode = 'create' | 'edit' | null;
const STATUSES: AgentStatus[] = ['Active', 'Inactive'];
const PAGE_SIZE = 10;

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './agent-list.html',
  styleUrl: './agent-list.css',
})
export class AgentListPage implements OnInit {
  readonly agents       = signal<Agent[]>([]);
  readonly loading      = signal(false);
  readonly saving       = signal(false);
  readonly modalMode    = signal<ModalMode>(null);
  readonly selected     = signal<Agent | null>(null);
  readonly searchQuery  = signal('');
  readonly filterStatus = signal('');
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly statuses     = STATUSES;
  readonly currentPage  = signal(0);

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    return this.agents().filter(a =>
      (!q || a.name.toLowerCase().includes(q) || a.producerCode.toLowerCase().includes(q)) &&
      (!s || a.status === s)
    );
  });

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));
  readonly paginated  = computed(() => {
    const page = Math.min(this.currentPage(), this.totalPages() - 1);
    return this.filtered().slice(page * PAGE_SIZE, page * PAGE_SIZE + PAGE_SIZE);
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Distribution & Party', route: '/party' },
    { label: 'Agents' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name: ['', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(150),
      noDigitsValidator(),
      Validators.pattern(/^[a-zA-Z\s\-'\.]+$/),
    ]],
    producerCode: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(50),
      Validators.pattern(/^[a-zA-Z0-9\-_]+$/),
    ]],
    region: ['', [
      Validators.maxLength(100),
      Validators.pattern(/^[a-zA-Z\s\-]*$/),
    ]],
    contactInfo: ['', [smartContactInfoValidator(), Validators.maxLength(500)]],
    status: ['Active' as AgentStatus, Validators.required],
  });

  constructor(private svc: PartyApiService) {
    effect(() => { this.filtered(); this.currentPage.set(0); });
  }

  ngOnInit(): void { this.load(); }

  goToPage(n: number): void { this.currentPage.set(Math.max(0, Math.min(n, this.totalPages() - 1))); }
  readonly pageSize = PAGE_SIZE;

  load(): void {
    this.loading.set(true);
    this.svc.getAgents().subscribe({
      next: res => {
        const d: any = res;
        this.agents.set(d?.data ?? d?.content ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.form.reset({ status: 'Active' });
    this.selected.set(null);
    this.modalMode.set('create');
  }

  openEdit(a: Agent): void {
    this.selected.set(a);
    this.form.patchValue({
      name:         a.name,
      producerCode: a.producerCode,
      region:       a.region ?? '',
      contactInfo:  a.contactInfo ?? '',
      status:       a.status,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload: Partial<Agent> = {
      name:         v.name!,
      producerCode: v.producerCode!,
      region:       v.region ?? '',
      contactInfo:  v.contactInfo ?? undefined,
      status:       v.status as AgentStatus,
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateAgent(this.selected()!.agentID, payload)
      : this.svc.createAgent(payload);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Agent saved successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', extractApiErrors(err));
      },
    });
  }

  toggleStatus(a: Agent): void {
    const req = a.status === 'Active'
      ? this.svc.deactivateAgent(a.agentID)
      : this.svc.activateAgent(a.agentID);
    req.subscribe({ next: () => { this.load(); this.flash('success', 'Status updated.'); } });
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
