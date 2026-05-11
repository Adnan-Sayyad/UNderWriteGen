import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PricingApiService } from '../../services/pricing-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-quote-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './quote-create.html',
  styleUrl: './quote-create.css',
})
export class QuoteCreatePage implements OnInit {
  /** true when submissionId came from URL param (navigated from submission page) */
  readonly submissionFromRoute = signal(false);
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger' | 'info'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home',         route: '/' },
    { label: 'Quotes',       route: '/pricing/quotes' },
    { label: 'Create Quote' },
  ];

  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  /**
   * Maps to backend CreateQuoteRequest DTO:
   *   submissionId   — entered manually or pre-filled from route param
   *   requestedBy    — pre-filled from logged-in user
   *   applyDiscounts — toggle
   *   applyTaxes     — toggle
   */
  readonly form = this.fb.group({
    submissionId:   ['', [Validators.required,
                          Validators.pattern('^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$')]],
    requestedBy:    ['', [Validators.required, Validators.maxLength(100)]],
    applyDiscounts: [true],
    applyTaxes:     [true],
  });

  constructor(
    private svc:    PricingApiService,
    private router: Router,
    private route:  ActivatedRoute,
  ) {}

  ngOnInit(): void {
    // Pre-fill submissionId if provided via route (e.g. navigated from submission page)
    const id = this.route.snapshot.paramMap.get('submissionId') ?? '';
    if (id) {
      this.submissionFromRoute.set(true);
      this.form.patchValue({ submissionId: id });
      this.form.controls.submissionId.disable();   // read-only when from route
    }

    // Pre-fill requestedBy from logged-in user
    const user = this.auth.currentUser();
    if (user) {
      this.form.patchValue({ requestedBy: user.name || user.email });
    }
  }

  generate(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    this.saving.set(true);
    const v = this.form.getRawValue();   // getRawValue() includes disabled controls

    this.svc.generateQuote({
      submissionId:   v.submissionId!,
      applyDiscounts: v.applyDiscounts ?? true,
      applyTaxes:     v.applyTaxes     ?? true,
      requestedBy:    v.requestedBy!,
    }).subscribe({
      next: quote => {
        this.saving.set(false);
        this.router.navigate(['/pricing/quotes', quote.quoteId]);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        const msg =
          err.error?.error   ??
          err.error?.message ??
          err.error?.title   ??
          'Quote generation failed. Please verify the Submission ID exists and has a valid risk score.';
        this.flash('danger', msg);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/pricing/quotes']);
  }

  private flash(type: 'success' | 'danger' | 'info', text: string): void {
    this.alertMsg.set({ type, text });
    if (type !== 'danger') setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
