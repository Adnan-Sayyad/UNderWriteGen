import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PremiumDistributionDto } from '../../models/reports.model';

@Component({
  selector: 'app-premium-distribution',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './premium-distribution.html',
  styleUrl: './premium-distribution.css',
})
export class PremiumDistributionChart {
  @Input() data: PremiumDistributionDto[] = [];

  get maxAvgPremium(): number {
    return Math.max(...this.data.map(d => d.avgPremium), 0.01);
  }

  get totalGross(): number {
    return this.data.reduce((s, d) => s + d.totalGrossPremium, 0);
  }

  get totalPolicies(): number {
    return this.data.reduce((s, d) => s + d.policyCount, 0);
  }

  barWidth(value: number): number {
    return this.maxAvgPremium > 0 ? Math.min(Math.round((value / this.maxAvgPremium) * 100), 100) : 0;
  }
}
