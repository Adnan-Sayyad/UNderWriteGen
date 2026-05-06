import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PricingApiService } from '../../services/pricing-api.service';
import { Quote } from '../../models/pricing.model';

@Component({
  selector: 'app-quote-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader],
  templateUrl: './quote-detail.html',
  styleUrl: './quote-detail.css',
})
export class QuoteDetailPage implements OnInit {
  readonly quoteId  = signal('');
  readonly quote    = signal<Quote | null>(null);
  readonly loading  = signal(true);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Quotes', route: '/pricing/quotes' },
    { label: 'Quote Detail' },
  ];

  readonly loadings  = computed(() => Object.entries(this.quote()?.loadingsJSON  ?? {}));
  readonly discounts = computed(() => Object.entries(this.quote()?.discountsJSON ?? {}));
  readonly taxes     = computed(() => Object.entries(this.quote()?.taxesJSON     ?? {}));

  constructor(
    private svc: PricingApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.quoteId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.svc.getQuote(id).subscribe({
      next: (res: any) => { this.quote.set(res?.data ?? res); this.loading.set(false); },
      error: () => { this.loading.set(false); this.flash('danger', 'Failed to load quote.'); },
    });
  }

  accept() {
    this.svc.acceptQuote(this.quoteId()).subscribe({
      next: (res: any) => {
        const updated: Quote = res?.data ?? res;
        this.quote.update(q => q ? { ...q, status: updated.status } : q);
        this.flash('success', 'Quote accepted.');
      },
      error: () => this.flash('danger', 'Failed to accept quote.'),
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', Offered: 'bg-primary', Accepted: 'bg-success', Expired: 'bg-warning text-dark',
    };
    return map[status] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
