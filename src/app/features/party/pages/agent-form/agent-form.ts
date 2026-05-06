import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PartyApiService } from '../../services/party-api.service';
import { Agent } from '../../models/party.model';

@Component({
  selector: 'app-agent-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './agent-form.html',
  styleUrl: './agent-form.css',
})
export class AgentFormPage implements OnInit {
  readonly agentId   = signal<string | null>(null);
  readonly loading   = signal(false);
  readonly saving    = signal(false);
  readonly alertMsg  = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Agents', route: '/party/agents' },
    { label: 'Agent Details' },
  ];

  readonly statuses = ['Active', 'Inactive', 'Suspended'];

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    name:         ['', Validators.required],
    producerCode: ['', Validators.required],
    region:       ['', Validators.required],
    status:       ['Active', Validators.required],
    email:        ['', [Validators.required, Validators.email]],
    phone:        [''],
    address:      [''],
  });

  constructor(
    private svc: PartyApiService,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.agentId.set(id);
      this.loading.set(true);
      this.svc.getAgent(id).subscribe({
        next: (res: any) => {
          const a: Agent = res?.data ?? res;
          this.form.patchValue({
            name:         a.name,
            producerCode: a.producerCode,
            region:       a.region,
            status:       a.status,
            email:        a.contactInfo?.email ?? '',
            phone:        a.contactInfo?.phone ?? '',
            address:      a.contactInfo?.address ?? '',
          });
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.flash('danger', 'Failed to load agent.'); },
      });
    }
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    const payload = {
      name:         v.name!,
      producerCode: v.producerCode!,
      region:       v.region!,
      status:       v.status as any,
      contactInfo:  { email: v.email!, phone: v.phone ?? '', address: v.address ?? '' },
    };
    this.saving.set(true);
    const req = this.agentId()
      ? this.svc.updateAgent(this.agentId()!, payload)
      : this.svc.createAgent(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.router.navigate(['/party/agents']); },
      error: () => { this.saving.set(false); this.flash('danger', 'Save failed. Please try again.'); },
    });
  }

  cancel() { this.router.navigate(['/party/agents']); }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
