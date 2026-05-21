import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { PartyApiService } from '../../services/party-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { Agent, AgentStatus } from '../../models/party.model';
import {
  noDigitsValidator,
  smartContactInfoValidator,
  extractApiErrors,
} from '../../../../shared/validators/custom-validators';

type ModalMode = 'create' | 'edit' | null;
const STATUSES: AgentStatus[] = ['Active', 'Inactive'];

@Component({
  selector: 'app-agent-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState, Pagination],
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
  readonly pageSize     = signal(10);

  readonly auth = inject(AuthService);

  /** Only Admin can add or edit agents. */
  readonly canManageAgents = computed(() => this.auth.hasRole('Admin'));
//Filtering and Sorting agents
  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    const list = this.agents().filter(a =>
      (!q || a.name.toLowerCase().includes(q) || a.producerCode.toLowerCase().includes(q)) &&
      (!s || a.status === s)
    );
    return list.sort((a, b) => {
      if (a.status !== b.status) return a.status === 'Active' ? -1 : 1;
      return a.name.localeCompare(b.name);
    });
  });
//Pagination
  readonly totalElements = computed(() => this.filtered().length);
  readonly totalPages    = computed(() => Math.max(1, Math.ceil(this.totalElements() / this.pageSize())));
  readonly paginated     = computed(() => {
    const size = this.pageSize();
    const page = Math.min(this.currentPage(), this.totalPages() - 1);
    return this.filtered().slice(page * size, page * size + size);
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
    region: ['', [
      Validators.maxLength(100),
      Validators.pattern(/^[a-zA-Z\s\-]*$/),
    ]],
    contactInfo: ['', [smartContactInfoValidator(), Validators.maxLength(500)]],
  });

  constructor(private svc: PartyApiService) {
    effect(() => { this.filtered(); this.currentPage.set(0); });
  }

  ngOnInit(): void { this.load(); }

  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }


//loads agents
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
    this.form.reset();
    this.selected.set(null);
    this.modalMode.set('create');
  }

  openEdit(a: Agent): void {
    this.selected.set(a);
    this.form.patchValue({
      name:        a.name,
      region:      a.region ?? '',
      contactInfo: a.contactInfo ?? '',
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload = {
      name:        v.name!,
      region:      v.region ?? '',
      contactInfo: v.contactInfo ?? undefined,
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
