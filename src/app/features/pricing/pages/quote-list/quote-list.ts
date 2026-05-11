import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { PricingApiService } from '../../services/pricing-api.service';
import { Quote, QuoteStatus } from '../../models/pricing.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

const STATUSES: QuoteStatus[] = ['Draft', 'Offered', 'Accepted', 'Expired'];

@Component({
  selector: 'app-quote-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './quote-list.html',
  styleUrl: './quote-list.css',
})
export class QuoteListPage implements OnInit {
  readonly quotes       = signal<Quote[]>([]);
  readonly loading      = signal(false);
  readonly filterStatus = signal('');
  readonly statuses     = STATUSES;
  readonly alertMsg     = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return this.quotes().filter(q => !s || q.status === s);
  });

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Pricing Console' },
    { label: 'Quotes' },
  ];

  constructor(private svc: PricingApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getQuotes(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.quotes.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  accept(q: Quote): void {
    this.svc.acceptQuote(q.quoteId).subscribe({
      next: () => { this.load(); this.flash('success', 'Quote accepted.'); },
      error: err => this.flash('danger', err?.error?.message ?? 'Accept failed.'),
    });
  }

  statusClass(s: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', Offered: 'bg-primary',
      Accepted: 'bg-success', Expired: 'bg-dark',
    };
    return map[s] ?? 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
