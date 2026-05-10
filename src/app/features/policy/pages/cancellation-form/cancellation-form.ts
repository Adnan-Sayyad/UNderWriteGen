import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PolicyApiService } from '../../services/policy-api.service';

@Component({
  selector: 'app-cancellation-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './cancellation-form.html',
  styleUrl: './cancellation-form.css',
})
export class CancellationFormPage implements OnInit {
  readonly policyId = signal('');
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policies', route: '/policy' },
    { label: 'Cancellation Request' },
  ];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    cancelReason:  ['', Validators.required],
    cancelDate:    ['', Validators.required],
    refundPremium: [0, [Validators.required, Validators.min(0)]],
  });

  constructor(
    private svc: PolicyApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.policyId.set(id);
    const today = new Date().toISOString().split('T')[0];
    this.form.patchValue({ cancelDate: today });
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const payload = {
      cancelReason:  v.cancelReason!,
      cancelDate:    v.cancelDate!,
      refundPremium: v.refundPremium!,
    };
    this.saving.set(true);
    this.svc.requestCancellation(this.policyId(), payload).subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/policy', this.policyId()]); },
      error: () => { this.saving.set(false); this.flash('danger', 'Failed to submit cancellation request.'); },
    });
  }

  cancel() { this.router.navigate(['/policy', this.policyId()]); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
