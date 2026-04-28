import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';

@Component({
  selector: 'app-referral-matrix',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, EmptyState],
  templateUrl: './referral-matrix.html',
  styleUrl: './referral-matrix.css',
})
export class ReferralMatrixPage {}
