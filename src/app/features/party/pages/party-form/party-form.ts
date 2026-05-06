import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PartyApiService } from '../../services/party-api.service';
import { CustomerParty } from '../../models/party.model';

@Component({
  selector: 'app-party-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './party-form.html',
  styleUrl: './party-form.css',
})
export class PartyFormPage implements OnInit {
  readonly partyId   = signal<string | null>(null);
  readonly loading   = signal(false);
  readonly saving    = signal(false);
  readonly alertMsg  = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Parties', route: '/party/parties' },
    { label: 'Party Details' },
  ];

  readonly partyTypes = ['Individual', 'Organization'];
  readonly segments   = ['Retail', 'SME', 'Corporate'];
  readonly statuses   = ['Active', 'Inactive'];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name:             ['', Validators.required],
    partyType:        ['Individual', Validators.required],
    segment:          ['Retail', Validators.required],
    status:           ['Active', Validators.required],
    dobIncorporation: [''],
    email:            ['', [Validators.required, Validators.email]],
    phone:            [''],
    address:          [''],
    city:             [''],
    state:            [''],
    pinCode:          [''],
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
          const p: CustomerParty = res?.data ?? res;
          this.form.patchValue({
            name:             p.name,
            partyType:        p.partyType,
            segment:          p.segment,
            status:           p.status,
            dobIncorporation: p.dobIncorporation ?? '',
            email:            p.contactInfo?.email ?? '',
            phone:            p.contactInfo?.phone ?? '',
            address:          p.contactInfo?.address ?? '',
            city:             p.contactInfo?.city ?? '',
            state:            p.contactInfo?.state ?? '',
            pinCode:          p.contactInfo?.pinCode ?? '',
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
    const payload = {
      name:             v.name!,
      partyType:        v.partyType as any,
      segment:          v.segment as any,
      status:           v.status as any,
      dobIncorporation: v.dobIncorporation ?? '',
      contactInfo: {
        email:   v.email!,
        phone:   v.phone ?? '',
        address: v.address ?? '',
        city:    v.city ?? '',
        state:   v.state ?? '',
        pinCode: v.pinCode ?? '',
      },
    };
    this.saving.set(true);
    const req = this.partyId()
      ? this.svc.updateParty(this.partyId()!, payload)
      : this.svc.createParty(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/party/parties']); },
      error: () => { this.saving.set(false); this.flash('danger', 'Save failed. Please try again.'); },
    });
  }

  cancel() { this.router.navigate(['/party/parties']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
