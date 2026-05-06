import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PolicyApiService } from '../../services/policy-api.service';

@Component({
  selector: 'app-endorsement-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './endorsement-form.html',
  styleUrl: './endorsement-form.css',
})
export class EndorsementFormPage implements OnInit {
  readonly policyId = signal('');
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policies', route: '/policy' },
    { label: 'Endorsement' },
  ];

  readonly endorsementTypes = ['MidTermChange', 'Address', 'Limit', 'Deductible', 'Beneficiary'];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    endorsementType: ['MidTermChange', Validators.required],
    effectiveDate:   ['', Validators.required],
    premiumDelta:    [0, Validators.required],
    changesSummary:  ['', Validators.required],
  });

  constructor(
    private svc: PolicyApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.policyId.set(id);
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const payload = {
      endorsementType: v.endorsementType as any,
      effectiveDate:   v.effectiveDate!,
      premiumDelta:    v.premiumDelta!,
      changesJSON:     { summary: v.changesSummary },
    };
    this.saving.set(true);
    this.svc.createEndorsement(this.policyId(), payload).subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/policy', this.policyId()]); },
      error: () => { this.saving.set(false); this.flash('danger', 'Failed to create endorsement.'); },
    });
  }

  cancel() { this.router.navigate(['/policy', this.policyId()]); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
