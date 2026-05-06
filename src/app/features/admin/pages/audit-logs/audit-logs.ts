import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { IamApiService, AuditLogDto } from '../../../../core/services/iam-api.service';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './audit-logs.html',
  styleUrl: './audit-logs.css',
})
export class AuditLogsPage implements OnInit {
  readonly logs        = signal<AuditLogDto[]>([]);
  readonly loading     = signal(false);
  readonly searchEmail = signal('');
  readonly filterRes   = signal('');

  readonly resources = ['Auth', 'UserManagement'];

  readonly filtered = computed(() => {
    const q   = this.searchEmail().toLowerCase();
    const res = this.filterRes();
    return this.logs().filter(l =>
      (!q   || l.email.toLowerCase().includes(q)) &&
      (!res || l.resource === res)
    );
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Admin', route: '/admin' },
    { label: 'Audit Logs' },
  ];

  private get adminId() { return this.auth.currentUser()?.userId ?? ''; }

  constructor(private iam: IamApiService, readonly auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.iam.getAuditLogs(this.adminId).subscribe({
      next: data => { this.logs.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  applyResourceFilter(res: string): void {
    this.filterRes.set(res);
    if (!res) { this.load(); return; }
    this.loading.set(true);
    this.iam.getAuditLogsByResource(res, this.adminId).subscribe({
      next: data => { this.logs.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }
}
