import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';

@Component({
  selector: 'app-risk-landing',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './risk-landing.html',
  styleUrl: './risk-landing.css',
})
export class RiskLandingPage {
  private readonly fb = inject(FormBuilder);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Risk & Evidence' },
  ];

  readonly form = this.fb.group({
    submissionId: ['', Validators.required],
  });

  readonly navigateTo = signal<'profile' | 'evidence'>('profile');

  constructor(private router: Router) {}

  goProfile(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.router.navigate(['/risk/profiles', this.form.value.submissionId]);
  }

  goEvidence(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.router.navigate(['/risk/evidence', this.form.value.submissionId]);
  }
}
