import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { PartyApiService } from '../../services/party-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { CustomerParty, PartyType, Segment, PartyStatus } from '../../models/party.model';
import {
  noDigitsValidator,
  smartContactInfoValidator,
  validDOBValidator,
  extractApiErrors,
} from '../../../../shared/validators/custom-validators';

type ModalMode = 'create' | 'edit' | null;
const PARTY_TYPES: PartyType[] = ['Individual', 'Corporation'];
const SEGMENTS: Segment[]       = ['Retail', 'SME', 'Corporate'];

@Component({
  selector: 'app-party-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState, Pagination],
  templateUrl: './party-list.html',
  styleUrl: './party-list.css',
})
export class PartyListPage implements OnInit {
  readonly parties     = signal<CustomerParty[]>([]);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly modalMode   = signal<ModalMode>(null);
  readonly selected    = signal<CustomerParty | null>(null);
  readonly searchQuery = signal('');
  readonly filterType  = signal('');
  readonly alertMsg    = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly partyTypes  = PARTY_TYPES;
  readonly segments    = SEGMENTS;
  readonly currentPage = signal(0);
  readonly pageSize    = signal(10);

  readonly auth = inject(AuthService);

  readonly isAdmin   = computed(() => this.auth.hasRole('Admin'));
  readonly isAgent   = computed(() => this.auth.hasRole('Agent'));
  readonly canAddParty = computed(() => this.isAgent() || this.isAdmin());

  canEditParty(p: CustomerParty): boolean {
    if (this.isAdmin()) return true;
    if (this.isAgent()) return p.createdByUserId === this.auth.currentUser()?.userId;
    return false;
  }

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const t = this.filterType();
    const list = this.parties().filter(p =>
      (!q || p.name.toLowerCase().includes(q)) && (!t || p.partyType === t)
    );
    return list.sort((a, b) => {
      if (a.status !== b.status) return a.status === 'Active' ? -1 : 1;
      return a.name.localeCompare(b.name);
    });
  });

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
    { label: 'Customer Parties' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name: ['', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(200),
      noDigitsValidator(),
      Validators.pattern(/^[a-zA-Z\s\-'\.&,]+$/),
    ]],
    partyType:        ['Individual' as PartyType, Validators.required],
    segment:          ['Retail' as Segment,       Validators.required],
    dOBIncorporation: ['', [validDOBValidator()]],
    contactInfo:      ['', [smartContactInfoValidator(), Validators.maxLength(500)]],
  });

  constructor(private svc: PartyApiService) {
    effect(() => { this.filtered(); this.currentPage.set(0); });
  }

  ngOnInit(): void { this.load(); }

  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }

  load(): void {
    this.loading.set(true);
    this.svc.getParties().subscribe({
      next: res => {
        const d: any = res;
        const raw: any[] = d?.data ?? d?.content ?? [];
        this.parties.set(raw.map((p: any) => ({
          ...p,
          dOBIncorporation: p.dOBIncorporation ?? p.dobIncorporation ?? p.DOBIncorporation ?? null,
          createdByUserId:  p.createdByUserId ?? p.CreatedByUserId ?? null,
        })));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.form.reset({ partyType: 'Individual', segment: 'Retail' });
    this.selected.set(null);
    this.modalMode.set('create');
  }

  openEdit(p: CustomerParty): void {
    this.selected.set(p);
    const rawDob: string = (p as any).dOBIncorporation ?? (p as any).dobIncorporation ?? '';
    const dobDate = rawDob ? rawDob.split('T')[0] : '';
    this.form.patchValue({
      name:             p.name,
      partyType:        p.partyType,
      segment:          p.segment,
      dOBIncorporation: dobDate,
      contactInfo:      p.contactInfo ?? '',
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload: Partial<CustomerParty> = {
      name:             v.name!,
      partyType:        v.partyType as PartyType,
      segment:          v.segment as Segment,
      dOBIncorporation: v.dOBIncorporation || null,
      contactInfo:      v.contactInfo ?? undefined,
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateParty(this.selected()!.partyID, payload)
      : this.svc.createParty(payload);
    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
        this.flash('success', 'Party saved successfully.');
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', extractApiErrors(err));
      },
    });
  }

  toggleStatus(p: CustomerParty): void {
    const req = p.status === 'Active'
      ? this.svc.deactivateParty(p.partyID)
      : this.svc.activateParty(p.partyID);
    req.subscribe({ next: () => { this.load(); this.flash('success', 'Status updated.'); } });
  }

  segmentClass(s: string): string {
    return s === 'Corporate' ? 'bg-primary' : s === 'SME' ? 'bg-warning text-dark' : 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
