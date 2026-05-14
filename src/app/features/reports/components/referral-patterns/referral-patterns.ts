import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReferralRateDto } from '../../models/reports.model';

@Component({
  selector: 'app-referral-patterns',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './referral-patterns.html',
  styleUrl: './referral-patterns.css',
})
export class ReferralPatternsChart {
  @Input() data: ReferralRateDto[] = [];

  get maxRate(): number {
    return Math.max(...this.data.map(d => d.referralRatePercent), 0.01);
  }

  get totalReferrals(): number {
    return this.data.reduce((s, d) => s + d.totalReferrals, 0);
  }

  get totalQuotes(): number {
    return this.data.reduce((s, d) => s + d.quotes, 0);
  }

  get overallRate(): number {
    return this.totalQuotes > 0 ? (this.totalReferrals / this.totalQuotes) * 100 : 0;
  }

  barWidth(value: number): number {
    return this.maxRate > 0 ? Math.min(Math.round((value / this.maxRate) * 100), 100) : 0;
  }

  riskLevel(rate: number): string {
    if (rate >= 40) return 'danger';
    if (rate >= 20) return 'warning';
    return 'success';
  }

  riskLabel(rate: number): string {
    if (rate >= 40) return 'High';
    if (rate >= 20) return 'Medium';
    return 'Low';
  }
}
