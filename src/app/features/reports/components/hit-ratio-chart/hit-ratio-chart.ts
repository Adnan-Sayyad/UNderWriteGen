import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HitRatioDto } from '../../models/reports.model';

@Component({
  selector: 'app-hit-ratio-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hit-ratio-chart.html',
  styleUrl: './hit-ratio-chart.css',
})
export class HitRatioChart {
  @Input() data: HitRatioDto[] = [];
  @Input() title = 'Quote Hit Ratio';

  get maxPct(): number {
    return Math.max(...this.data.map(d => d.hitRatioPercent), 0.01);
  }

  barWidth(value: number): number {
    return this.maxPct > 0 ? Math.min(Math.round((value / this.maxPct) * 100), 100) : 0;
  }

  get totalQuotes(): number   { return this.data.reduce((s, d) => s + d.quotes, 0); }
  get totalBound(): number    { return this.data.reduce((s, d) => s + d.boundPolicies, 0); }
  get overallHitRatio(): number {
    return this.totalQuotes > 0 ? (this.totalBound / this.totalQuotes) * 100 : 0;
  }
}
