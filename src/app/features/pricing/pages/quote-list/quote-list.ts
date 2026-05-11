import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PricingApiService } from '../../services/pricing-api.service';
import { Quote, QuoteStatus } from '../../models/pricing.model';
import { HttpErrorResponse } from '@angular/common/http';

const SESSION_KEY = 'uwpro_recent_quotes';
const MAX_RECENT  = 20;

export interface RecentQuote {
  quoteId: string;
  quoteRef?: string;
  submissionId: string;
  versionNo: number;
  basePremium: number;
  totalPremium: number;
  validUntil: string;
  status: QuoteStatus;
  searchedAt: string;
}

@Component({
  selector: 'app-quote-list',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, EmptyState],
  // imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './quote-list.html',
  styleUrl: './quote-list.css',
})
export class QuoteListPage implements OnInit {
  /* ── search state ───────────────────────────────────────────────────── */
  readonly searchInput    = signal('');
  readonly searching      = signal(false);
  readonly searchResult   = signal<Quote | null>(null);
  readonly searchError    = signal('');
  readonly allVersions    = signal<Quote[]>([]);
  readonly loadingVersions = signal(false);

  /* ── recent quotes ──────────────────────────────────────────────────── */
  readonly recentQuotes = signal<RecentQuote[]>([]);
  readonly filterStatus = signal<QuoteStatus | ''>('');
  readonly accepting    = signal<string | null>(null);
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly ALL_STATUSES: QuoteStatus[] = ['Draft', 'Presented', 'Accepted', 'Declined', 'Expired'];

  /* ── computed ───────────────────────────────────────────────────────── */
  readonly filteredRecent = computed(() => {
    const s = this.filterStatus();
    return s ? this.recentQuotes().filter(q => q.status === s) : this.recentQuotes();
  });

  /** Mini stats drawn from session history */
  readonly stats = computed(() => {
    const list = this.recentQuotes();
    return {
      total:     list.length,
      draft:     list.filter(q => q.status === 'Draft').length,
      presented: list.filter(q => q.status === 'Presented').length,
      accepted:  list.filter(q => q.status === 'Accepted').length,
      declined:  list.filter(q => q.status === 'Declined').length,
      expired:   list.filter(q => q.status === 'Expired').length,
    };
  });

  readonly breadcrumbs = [
    { label: 'Home',            route: '/' },
    { label: 'Pricing Console', route: '/pricing' },
    { label: 'Quotes' },
  ];

  constructor(private svc: PricingApiService) {}

  ngOnInit(): void { this.loadRecent(); }

  /* ── search ─────────────────────────────────────────────────────────── */
  search(): void {
    const id = this.searchInput().trim();
    if (!id) return;
    this.searching.set(true);
    this.searchResult.set(null);
    this.searchError.set('');

    this.svc.getLatestQuoteForSubmission(id).subscribe({
      next: quote => {
        this.searchResult.set(quote);
        this.searching.set(false);
        this.addToRecent(quote);
      },
      error: (err: HttpErrorResponse) => {
        this.searching.set(false);
        this.searchError.set(
          err.status === 404
            ? `No quote found for submission "${id.slice(0, 8)}…"`
            : (err.error?.error ?? 'Search failed. Check the Submission ID and try again.')
        );
      },
    });
  }

  clearSearch(): void {
    this.searchInput.set('');
    this.searchResult.set(null);
    this.searchError.set('');
    this.allVersions.set([]);
  }

  loadAllVersions(submissionId: string): void {
    this.loadingVersions.set(true);
    this.svc.getQuotesBySubmission(submissionId).subscribe({
      next: quotes => {
        this.allVersions.set(quotes);
        this.loadingVersions.set(false);
        quotes.forEach(q => this.addToRecent(q));
      },
      error: () => this.loadingVersions.set(false),
    });
  }

  onSearchKey(event: KeyboardEvent): void {
    if (event.key === 'Enter') this.search();
  }

  /* ── accept ─────────────────────────────────────────────────────────── */
  accept(quoteId: string): void {
    this.accepting.set(quoteId);
    this.svc.acceptQuote(quoteId).subscribe({
      next: () => {
        this.accepting.set(null);
        this.flash('success', 'Quote accepted successfully.');
        if (this.searchResult()?.quoteId === quoteId) {
          this.svc.getQuote(quoteId).subscribe(q => {
            this.searchResult.set(q);
            this.updateRecentStatus(quoteId, q.status);
          });
        } else {
          this.updateRecentStatus(quoteId, 'Accepted');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.accepting.set(null);
        this.flash('danger', err.error?.error ?? 'Failed to accept quote.');
      },
    });
  }

  /* ── history management ─────────────────────────────────────────────── */
  removeRecent(quoteId: string): void {
    const updated = this.recentQuotes().filter(q => q.quoteId !== quoteId);
    this.recentQuotes.set(updated);
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(updated));
  }

  clearRecent(): void {
    this.recentQuotes.set([]);
    sessionStorage.removeItem(SESSION_KEY);
  }

  /* ── helpers ────────────────────────────────────────────────────────── */
  statusClass(s: string): string {
    const map: Record<string, string> = {
      Draft:     'badge-status badge-draft',
      Presented: 'badge-status badge-presented',
      Accepted:  'badge-status badge-accepted',
      Declined:  'badge-status badge-declined',
      Expired:   'badge-status badge-expired',
    };
    return map[s] ?? 'badge-status badge-draft';
  }

  isExpired(validUntil: string): boolean {
    return new Date(validUntil) < new Date();
  }

  daysLeft(validUntil: string): number {
    return Math.ceil((new Date(validUntil).getTime() - Date.now()) / 86_400_000);
  }

  daysLeftLabel(validUntil: string): string {
    const d = this.daysLeft(validUntil);
    if (d < 0)  return `Expired ${Math.abs(d)} day${Math.abs(d) === 1 ? '' : 's'} ago`;
    if (d === 0) return 'Expires today';
    if (d <= 7)  return `${d} day${d === 1 ? '' : 's'} left`;
    return '';   // no label needed when plenty of time remains
  }

  /* ── private ────────────────────────────────────────────────────────── */
  private loadRecent(): void {
    try {
      const raw = sessionStorage.getItem(SESSION_KEY);
      this.recentQuotes.set(raw ? JSON.parse(raw) : []);
    } catch { this.recentQuotes.set([]); }
  }

  private addToRecent(q: Quote): void {
    const entry: RecentQuote = {
      quoteId: q.quoteId, quoteRef: q.quoteRef, submissionId: q.submissionId, versionNo: q.versionNo,
      basePremium: q.basePremium, totalPremium: q.totalPremium,
      validUntil: q.validUntil, status: q.status,
      searchedAt: new Date().toISOString(),
    };
    const deduped = this.recentQuotes().filter(r => r.quoteId !== q.quoteId);
    const updated = [entry, ...deduped].slice(0, MAX_RECENT);
    this.recentQuotes.set(updated);
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(updated));
  }

  private updateRecentStatus(quoteId: string, status: QuoteStatus): void {
    const updated = this.recentQuotes().map(r =>
      r.quoteId === quoteId ? { ...r, status } : r
    );
    this.recentQuotes.set(updated);
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(updated));
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
