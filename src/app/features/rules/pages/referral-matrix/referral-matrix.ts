import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RulesApiService } from '../../services/rules-api.service';
import { ReferralMatrix } from '../../models/rules.model';

@Component({
  selector: 'app-referral-matrix',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './referral-matrix.html',
  styleUrl: './referral-matrix.css',
})
export class ReferralMatrixPage implements OnInit {
  readonly matrices       = signal<ReferralMatrix[]>([]);
  readonly loading        = signal(true);
  readonly filterProduct  = signal('');

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Rules', route: '/rules/list' },
    { label: 'Referral Matrix' },
  ];

  readonly productLines = ['', 'Life', 'Health', 'PnC', 'Commercial'];

  readonly filtered = computed(() => {
    const p = this.filterProduct();
    return p ? this.matrices().filter(m => m.productLine === p) : this.matrices();
  });

  constructor(private svc: RulesApiService) {}

  ngOnInit() { this.load(); }

  load() {
    const p = this.filterProduct() || undefined;
    this.loading.set(true);
    this.svc.getMatrices(p).subscribe({
      next: (res: any) => { this.matrices.set(res?.data ?? res ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  authorityClass(a: string): string {
    const map: Record<string, string> = {
      UW1: 'bg-primary', UW2: 'bg-info text-dark', UWManager: 'bg-warning text-dark', Committee: 'bg-danger',
    };
    return map[a] ?? 'bg-secondary';
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : 'bg-secondary';
  }
}
