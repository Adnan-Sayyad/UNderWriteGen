import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { RiskApiService } from '../../services/risk-api.service';
import { RiskProfile, RiskType } from '../../models/risk.model';

const RISK_TYPES: RiskType[] = ['Life', 'Health', 'Property', 'Auto', 'Marine', 'GL'];

@Component({
  selector: 'app-risk-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './risk-profile.html',
  styleUrl: './risk-profile.css',
})
export class RiskProfilePage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly profile     = signal<RiskProfile | null>(null);
  readonly loading     = signal(false);
  readonly saving      = signal(false);
  readonly submissionId = signal('');
  readonly alertMsg    = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly riskTypes   = RISK_TYPES;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Risk & Evidence' },
  ];

  readonly form = this.fb.group({
    submissionId: ['', Validators.required],
    riskType:     ['Life' as RiskType, Validators.required],
    riskNotes:    [''],
  });

  readonly searchForm = this.fb.group({
    submissionId: ['', Validators.required],
  });

  constructor(
    private svc: RiskApiService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('submissionId');
    if (id) { this.submissionId.set(id); this.loadProfile(id); }
  }

  search(): void {
    if (this.searchForm.invalid) { this.searchForm.markAllAsTouched(); return; }
    const id = this.searchForm.value.submissionId!;
    this.submissionId.set(id);
    this.loadProfile(id);
  }

  private loadProfile(id: string): void {
    this.loading.set(true);
    this.svc.getRiskProfile(id).subscribe({
      next: res => {
        const d: any = res;
        const p = d?.data ?? null;
        this.profile.set(p);
        if (p) {
          this.form.patchValue({ submissionId: p.submissionId, riskType: p.riskType, riskNotes: p.riskNotes });
        } else {
          this.form.patchValue({ submissionId: id });
        }
        this.loading.set(false);
      },
      error: () => { this.profile.set(null); this.loading.set(false); },
    });
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    this.svc.saveRiskProfile(v.submissionId!, {
      riskType: v.riskType as RiskType, riskNotes: v.riskNotes ?? '', attributesJSON: {},
    }).subscribe({
      next: res => {
        const d: any = res;
        this.profile.set(d?.data ?? null);
        this.saving.set(false);
        this.flash('success', 'Risk profile saved.');
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Save failed.'); },
    });
  }

  riskBandClass(band: string): string {
    return band === 'High' ? 'bg-danger' : band === 'Medium' ? 'bg-warning text-dark' : 'bg-success';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
