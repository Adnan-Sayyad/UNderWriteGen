import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PartyApiService } from '../../services/party-api.service';
import { CustomerParty } from '../../models/party.model';
import {
  noDigitsValidator,
  smartContactInfoValidator,
  validDOBValidator,
  extractApiErrors,
} from '../../../../shared/validators/custom-validators';

@Component({
  selector: 'app-party-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './party-form.html',
  styleUrl: './party-form.css',
})
export class PartyFormPage implements OnInit {
  readonly partyId  = signal<string | null>(null);
  readonly loading  = signal(false);
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Parties', route: '/party/parties' },
    { label: 'Party Details' },
  ];

  readonly partyTypes = ['Individual', 'Corporation'];   // backend expects 'Corporation'
  readonly segments   = ['Retail', 'SME', 'Corporate'];
  readonly statuses   = ['Active', 'Inactive'];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name: ['', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(200),
      noDigitsValidator(),
      Validators.pattern(/^[a-zA-Z\s\-'\.&,]+$/),
    ]],
    partyType:        ['Individual', Validators.required],
    segment:          ['Retail',     Validators.required],
    status:           ['Active',     Validators.required],
    dOBIncorporation: ['', [validDOBValidator()]],
    contactInfo:      ['', [smartContactInfoValidator(), Validators.maxLength(500)]],
  });

  constructor(
    private svc: PartyApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.partyId.set(id);
      this.loading.set(true);
      this.svc.getParty(id).subscribe({
        next: (res: any) => {
          const p: any = res?.data ?? res;
          const rawDob: string = p.dOBIncorporation ?? p.dobIncorporation ?? p.DOBIncorporation ?? '';
          const dobDate = rawDob ? rawDob.split('T')[0] : '';
          this.form.patchValue({
            name:             p.name,
            partyType:        p.partyType,
            segment:          p.segment,
            status:           p.status,
            dOBIncorporation: dobDate,
            contactInfo:      p.contactInfo ?? '',
          });
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.flash('danger', 'Failed to load party.'); },
      });
    }
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const payload: Partial<CustomerParty> = {
      name:             v.name!,
      partyType:        v.partyType as any,
      segment:          v.segment as any,
      status:           v.status as any,
      dOBIncorporation: v.dOBIncorporation || null,
      contactInfo:      v.contactInfo || null,
    };
    this.saving.set(true);
    const req = this.partyId()
      ? this.svc.updateParty(this.partyId()!, payload)
      : this.svc.createParty(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/party/parties']); },
      error: err => { this.saving.set(false); this.flash('danger', extractApiErrors(err)); },
    });
  }

  cancel() { this.router.navigate(['/party/parties']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
