import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PartyApiService } from '../../services/party-api.service';
import { Agent, AgentStatus } from '../../models/party.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

type ModalMode = 'create' | 'edit' | null;
const STATUSES: AgentStatus[] = ['Active', 'Inactive', 'Suspended'];

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './agent-list.html',
  styleUrl: './agent-list.css',
})
export class AgentListPage implements OnInit {
  readonly agents      = signal<Agent[]>([]);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly modalMode   = signal<ModalMode>(null);
  readonly selected    = signal<Agent | null>(null);
  readonly searchQuery = signal('');
  readonly filterStatus = signal('');
  readonly alertMsg    = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly statuses    = STATUSES;

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    return this.agents().filter(a =>
      (!q || a.name.toLowerCase().includes(q) || a.producerCode.toLowerCase().includes(q)) &&
      (!s || a.status === s)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Parties', route: '/party' },
    { label: 'Agents' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name:        ['', Validators.required],
    producerCode:['', Validators.required],
    region:      ['', Validators.required],
    email:       ['', [Validators.required, Validators.email]],
    phone:       ['', Validators.required],
    status:      ['Active' as AgentStatus, Validators.required],
  });

  constructor(private svc: PartyApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAgents(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.agents.set(d?.content ?? d?.data ?? []);
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
      name: a.name, producerCode: a.producerCode,
      region: a.region, status: a.status,
      email: a.contactInfo?.email, phone: a.contactInfo?.phone,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload: Partial<Agent> = {
      name: v.name!, producerCode: v.producerCode!, region: v.region!,
      status: v.status as AgentStatus,
      contactInfo: { email: v.email!, phone: v.phone! },
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateAgent(this.selected()!.agentId, payload)
      : this.svc.createAgent(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Agent saved.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : s === 'Suspended' ? 'bg-danger' : 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
