import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Quote } from '../../models/pricing.model';

@Component({
  selector: 'app-quote-summary-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './quote-summary-card.html',
  styleUrl: './quote-summary-card.css',
})
export class QuoteSummaryCard {
  @Input({ required: true }) quote!: Quote;
  @Input() showActions = true;
  @Input() accepting   = false;

  @Output() accept  = new EventEmitter<string>();
  @Output() viewDetail = new EventEmitter<string>();

  onAccept():     void { this.accept.emit(this.quote.quoteId); }
  onViewDetail(): void { this.viewDetail.emit(this.quote.quoteId); }

  get statusClass(): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', Presented: 'bg-primary',
      Accepted: 'bg-success', Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[this.quote.status] ?? 'bg-secondary';
  }

  get isExpired(): boolean {
    return new Date(this.quote.validUntil) < new Date();
  }

  get daysUntilExpiry(): number {
    const diff = new Date(this.quote.validUntil).getTime() - Date.now();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }
}
