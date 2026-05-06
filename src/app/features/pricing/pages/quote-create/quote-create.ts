import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PricingApiService } from '../../services/pricing-api.service';

@Component({
  selector: 'app-quote-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './quote-create.html',
  styleUrl: './quote-create.css',
})
export class QuoteCreatePage implements OnInit {
  readonly submissionId = signal('');
  readonly saving       = signal(false);
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Quotes', route: '/pricing/quotes' },
    { label: 'New Quote' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    basePremium:  [0, [Validators.required, Validators.min(0)]],
    totalPremium: [0, [Validators.required, Validators.min(0)]],
    validUntil:   ['', Validators.required],
    deductibles:  [0],
    exclusions:   [''],
  });

  constructor(
    private svc: PricingApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('submissionId') ?? '';
    this.submissionId.set(id);
    const thirtyDays = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
    this.form.patchValue({ validUntil: thirtyDays });
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const exclusionList = v.exclusions ? v.exclusions.split(',').map((s: string) => s.trim()).filter(Boolean) : [];
    const payload = {
      submissionId:  this.submissionId(),
      basePremium:   v.basePremium!,
      totalPremium:  v.totalPremium!,
      validUntil:    v.validUntil!,
      loadingsJSON:  {},
      discountsJSON: {},
      taxesJSON:     {},
      termsJSON:     { deductibles: v.deductibles ?? undefined, exclusions: exclusionList },
    };
    this.saving.set(true);
    this.svc.createQuote(payload).subscribe({
      next: (res: any) => {
        this.saving.set(false);
        const created = res?.data ?? res;
        this.router.navigate(['/pricing/quotes', created?.quoteId ?? '']);
      },
      error: () => { this.saving.set(false); this.flash('danger', 'Failed to create quote.'); },
    });
  }

  cancel() { this.router.navigate(['/pricing/quotes']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
