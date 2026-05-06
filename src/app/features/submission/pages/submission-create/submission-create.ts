import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SubmissionApiService } from '../../services/submission-api.service';
import { ProductLine } from '../../models/submission.model';
import { signal } from '@angular/core';

const PRODUCT_LINES: ProductLine[] = ['Life', 'Health', 'PnC', 'Commercial'];

@Component({
  selector: 'app-submission-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './submission-create.html',
  styleUrl: './submission-create.css',
})
export class SubmissionCreatePage {
  private readonly fb = inject(FormBuilder);

  readonly saving    = signal(false);
  readonly alertMsg  = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly productLines = PRODUCT_LINES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'New Submission' },
  ];

  readonly form = this.fb.group({
    partyId:       ['', Validators.required],
    agentId:       ['', Validators.required],
    productLine:   ['Life' as ProductLine, Validators.required],
    inceptionDate: ['', Validators.required],
  });

  constructor(
    private svc: SubmissionApiService,
    private router: Router,
  ) {}

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    this.svc.create({
      partyId: v.partyId!,
      agentId: v.agentId!,
      productLine: v.productLine as ProductLine,
      inceptionDate: v.inceptionDate!,
      coverageJSON: {},
    }).subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/submissions']); },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Create failed.'); },
    });
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
