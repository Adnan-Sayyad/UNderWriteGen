import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, SubmissionStatus, ProductLine } from '../../models/submission.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { PartyApiService } from '../../../party/services/party-api.service';
const STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];
const PRODUCT_LINES: ProductLine[] = ['Life', 'Health', 'PnC', 'Commercial'];

@Component({
  selector: 'app-submission-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './submission-list.html',
  styleUrl: './submission-list.css',
})
export class SubmissionListPage implements OnInit {
  readonly submissions   = signal<Submission[]>([]);
  readonly loading       = signal(false);
  readonly searchQuery   = signal('');
  readonly filterStatus  = signal('');
  readonly filterProduct = signal('');
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);

  readonly statuses     = STATUSES;
  readonly productLines = PRODUCT_LINES;

  // Agent's own agentID from Agents table (different from auth userId)
  readonly myAgentId    = signal<string | null>(null);
  readonly isAgentRole  = computed(() => this.auth.currentUser()?.role === 'Agent');

  readonly filtered = computed(() => {
    const q         = this.searchQuery().toLowerCase();
    const s         = this.filterStatus();
    const p         = this.filterProduct();
    const agentId   = this.myAgentId();
    const isAgent   = this.auth.currentUser()?.role === 'Agent';

    return this.submissions().filter(sub => {
      // Agent: only show their own submissions.
      // If agentId is still resolving (null), hide everything — safer than showing all.
      if (isAgent) {
        if (!agentId) return false;
        if (sub.agentId !== agentId) return false;
      }
      return (
        (!q || sub.submissionId.toLowerCase().includes(q) || sub.partyId?.toLowerCase().includes(q)) &&
        (!s || sub.status === s) &&
        (!p || sub.productLine === p)
      );
    });
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions' },
  ];

  private readonly auth = inject(AuthService);

  constructor(
    private svc: SubmissionApiService,
    private router: Router,
    private partySvc: PartyApiService,
  ) {}

  logBreach(submissionId: string): void {
    this.router.navigate(['/compliance/authority-breaches'], { queryParams: { submissionId } });
  }

  logException(submissionId: string): void {
    this.router.navigate(['/compliance/exceptions'], { queryParams: { submissionId } });
  }

  newChecklist(submissionId: string): void {
    this.router.navigate(['/compliance/checklists'], { queryParams: { submissionId } });
  }

  ngOnInit(): void {
    const user = this.auth.currentUser();
    if (user?.role === 'Agent') {
      const cacheKey = `uwpro_agent_id_${user.userId}`;
      const cached   = localStorage.getItem(cacheKey)
                    ?? localStorage.getItem('uwpro_my_agent_id');

      if (cached) {
        // Already known — set and load immediately
        this.myAgentId.set(cached);
        localStorage.setItem(cacheKey, cached);
        localStorage.setItem('uwpro_my_agent_id', cached);
        this.load();
      } else {
        // Not cached yet — look up this agent's record by name from the agents list,
        // then cache it before loading submissions so the filter works on first visit.
        this.loading.set(true);
        const hint = user.name ?? user.email?.split('@')[0] ?? '';
        this.partySvc.getAgents(hint).subscribe({
          next: (res: any) => {
            const agents: any[] = Array.isArray(res) ? res : (res?.data ?? res?.content ?? []);
            if (agents.length > 0) {
              // Take the first result — the search is already scoped to this agent's name
              const agentId = agents[0].agentID ?? agents[0].agentId ?? '';
              if (agentId) {
                this.myAgentId.set(agentId);
                localStorage.setItem(cacheKey, agentId);
                localStorage.setItem('uwpro_my_agent_id', agentId);
              }
            }
            this.loading.set(false);
            this.load();
          },
          error: () => {
            // Could not resolve agentId — load anyway; filter will show nothing
            // rather than showing all submissions
            this.loading.set(false);
            this.load();
          },
        });
      }
    } else {
      this.load();
    }
  }

  load(page = this.currentPage()): void {
    this.loading.set(true);
    const isAgent = this.isAgentRole();
    const agentId = this.myAgentId();

    // Agent: load all in one shot so client-side filter sees every record
    const req: any = isAgent
      ? { page: 0, size: 500, sort: 'createdDate', direction: 'desc' }
      : { page, size: this.pageSize(), sort: 'createdDate', direction: 'desc' };

    // Also send agentID to backend if it supports filtering (reduces payload)
    const filters: Record<string, string> = {};
    if (isAgent && agentId) filters['agentID'] = agentId;

    this.svc.getAll(req, Object.keys(filters).length ? filters : undefined).subscribe({
      next: res => {
        this.submissions.set(res.content ?? []);
        this.totalPages.set(res.totalPages ?? 1);
        this.totalElements.set(res.totalElements ?? 0);
        this.currentPage.set(isAgent ? 0 : page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark',
      UnderReview: 'bg-warning text-dark', Quoted: 'bg-primary',
      Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[status] ?? 'bg-secondary';
  }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }
}
