import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { IamApiService, AuditLogDto } from '../../../../core/services/iam-api.service';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.css',
})
export class AuditLogPage implements OnInit {
  readonly logs         = signal<AuditLogDto[]>([]);
  readonly loading      = signal(false);
  readonly searchEmail  = signal('');
  readonly filterAction = signal('');
  readonly filterRes    = signal('');
  readonly expanded     = signal<string | null>(null);

  readonly resources = ['Auth', 'UserManagement'];

  readonly uniqueActions = computed(() =>
    [...new Set(this.logs().map(l => l.action))].sort()
  );

  readonly filtered = computed(() => {
    const q   = this.searchEmail().toLowerCase();
    const act = this.filterAction();
    const res = this.filterRes();
    return this.logs().filter(l =>
      (!q   || l.email.toLowerCase().includes(q)) &&
      (!act || l.action === act) &&
      (!res || l.resource === res)
    );
  });

  // Pagination
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalElements = computed(() => this.filtered().length);
  readonly totalPages    = computed(() => Math.max(1, Math.ceil(this.totalElements() / this.pageSize())));
  readonly paged         = computed(() => {
    const size  = this.pageSize();
    const page  = Math.min(this.currentPage(), this.totalPages() - 1);
    const start = page * size;
    return this.filtered().slice(start, start + size);
  });

  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Compliance', route: '/compliance' },
    { label: 'Audit Log' },
  ];

  private get adminId() { return this.auth.currentUser()?.userId ?? ''; }

  constructor(private iam: IamApiService, readonly auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.iam.getAuditLogs(this.adminId).subscribe({
      next: (data: AuditLogDto[]) => { this.logs.set(data); this.loading.set(false); },
      error: ()                   => this.loading.set(false),
    });
  }

  toggleExpand(id: string): void {
    this.expanded.set(this.expanded() === id ? null : id);
  }

  formatMeta(raw?: string): string {
    if (!raw) return '—';
    try { return JSON.stringify(JSON.parse(raw), null, 2); }
    catch { return raw; }
  }
}
