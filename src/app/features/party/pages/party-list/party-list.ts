import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PartyApiService } from '../../services/party-api.service';
import { CustomerParty, PartyType, Segment, PartyStatus } from '../../models/party.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

type ModalMode = 'create' | 'edit' | null;
const PARTY_TYPES: PartyType[] = ['Individual', 'Organization'];
const SEGMENTS: Segment[] = ['Retail', 'SME', 'Corporate'];

@Component({
  selector: 'app-party-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
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
  readonly alertMsg    = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly partyTypes  = PARTY_TYPES;
  readonly segments    = SEGMENTS;

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const t = this.filterType();
    return this.parties().filter(p =>
      (!q || p.name.toLowerCase().includes(q)) && (!t || p.partyType === t)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Parties', route: '/party' },
    { label: 'Customer Parties' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name:             ['', Validators.required],
    partyType:        ['Individual' as PartyType, Validators.required],
    dobIncorporation: ['', Validators.required],
    segment:          ['Retail' as Segment, Validators.required],
    email:            ['', [Validators.required, Validators.email]],
    phone:            ['', Validators.required],
    address:          [''],
  });

  constructor(private svc: PartyApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getParties(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.parties.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.form.reset({ partyType: 'Individual', segment: 'Retail' }); this.modalMode.set('create'); }

  openEdit(p: CustomerParty): void {
    this.selected.set(p);
    this.form.patchValue({
      name: p.name, partyType: p.partyType, dobIncorporation: p.dobIncorporation,
      segment: p.segment, email: p.contactInfo?.email, phone: p.contactInfo?.phone,
      address: p.contactInfo?.address,
    });
    this.modalMode.set('edit');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    const payload: Partial<CustomerParty> = {
      name: v.name!, partyType: v.partyType as PartyType,
      dobIncorporation: v.dobIncorporation!, segment: v.segment as Segment,
      contactInfo: { email: v.email!, phone: v.phone!, address: v.address ?? '' },
    };
    const req = this.modalMode() === 'edit'
      ? this.svc.updateParty(this.selected()!.partyId, payload)
      : this.svc.createParty(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.load(); this.flash('success', 'Party saved.'); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  segmentClass(s: string): string {
    return s === 'Corporate' ? 'bg-primary' : s === 'SME' ? 'bg-warning text-dark' : 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
