import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PolicyApiService } from '../../services/policy-api.service';
import { Policy, PolicyStatus } from '../../models/policy.model';

const STATUSES: PolicyStatus[] = ['Active', 'Cancelled', 'Expired'];
const PRODUCT_LINES = ['Life', 'Health', 'PnC', 'Commercial'];
const PAGE_SIZE = 10;

@Component({
  selector: 'app-policy-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.css',
})
export class PolicyListPage implements OnInit {
  readonly policies      = signal<Policy[]>([]);
  readonly loading       = signal(false);
  readonly filterStatus  = signal('');
  readonly searchQuery   = signal('');
  readonly statuses      = STATUSES;
  readonly productLines  = PRODUCT_LINES;
  readonly currentPage   = signal(0);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly showBindModal = signal(false);
  readonly binding       = signal(false);

  // Bind form fields
  readonly bindSubId         = signal('');
  readonly bindPolicyNumber  = signal('');
  readonly bindProductLine   = signal('Life');
  readonly bindInceptionDate = signal('');
  readonly bindExpiryDate    = signal('');

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    return this.policies().filter(p =>
      (!q || p.policyNumber?.toLowerCase().includes(q)) && (!s || p.status === s)
    );
  });

  readonly pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i)
  );

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policy Desk' },
  ];

  constructor(private svc: PolicyApiService) {}

  ngOnInit(): void { this.load(); }

  load(page = 0): void {
    this.loading.set(true);
    const req: any = { page, size: PAGE_SIZE, sort: 'createdDate', direction: 'desc' };
    this.svc.getAll(req).subscribe({
      next: (res: any) => {
        this.policies.set(res?.content ?? res?.data ?? []);
        this.totalPages.set(res?.totalPages ?? 1);
        this.totalElements.set(res?.totalElements ?? 0);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goToPage(page: number): void {
    if (page < 0 || page >= this.totalPages()) return;
    this.load(page);
  }

  openBindModal(): void {
    this.showBindModal.set(true);
    this.bindSubId.set('');
    this.bindPolicyNumber.set('');
    this.bindProductLine.set('Life');
    this.bindInceptionDate.set('');
    this.bindExpiryDate.set('');
  }

  closeBindModal(): void { this.showBindModal.set(false); }

  bindPolicy(): void {
    const sid = this.bindSubId().trim();
    const pn  = this.bindPolicyNumber().trim();
    const pl  = this.bindProductLine();
    const id  = this.bindInceptionDate();
    const ed  = this.bindExpiryDate();
    if (!sid || !pn || !pl || !id || !ed) {
      this.flash('danger', 'All fields are required to bind a policy.');
      return;
    }
    this.binding.set(true);
    this.svc.bind(sid, pn, pl, id, ed).subscribe({
      next: () => {
        this.binding.set(false);
        this.closeBindModal();
        this.flash('success', 'Policy bound successfully.');
        this.load(0);
      },
      error: (err: any) => {
        this.binding.set(false);
        this.flash('danger', err?.error?.message ?? 'Failed to bind policy.');
      },
    });
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : s === 'Cancelled' ? 'bg-danger' : 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
