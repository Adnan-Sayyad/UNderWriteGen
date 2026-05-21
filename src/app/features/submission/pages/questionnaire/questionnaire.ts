import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Questionnaire, ProductLine } from '../../models/submission.model';

@Component({
  selector: 'app-questionnaire',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './questionnaire.html',
  styleUrl: './questionnaire.css',
})
export class QuestionnairePage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly submissionId  = signal('');
  readonly productLine   = signal<ProductLine | ''>('');
  readonly questionnaire = signal<Questionnaire | null>(null);
  readonly loading       = signal(true);
  readonly saving        = signal(false);
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'Questionnaire' },
  ];

  // ── Life ────────────────────────────────────────────────────────────
  readonly lifeForm = this.fb.group({
    sumInsured:            [0,           [Validators.required, Validators.min(1)]],
    policyTenureMonths:    [12,          Validators.required],
    occupationType:        ['',          Validators.required],
    smokingStatus:         ['Non-Smoker', Validators.required],
    annualIncome:          [0,           [Validators.required, Validators.min(0)]],
    existingConditions:    ['None'],
    familyMedicalHistory:  ['None'],
    additionalNotes:       [''],
  });

  // ── Health ──────────────────────────────────────────────────────────
  readonly healthForm = this.fb.group({
    sumInsured:                  [0,  [Validators.required, Validators.min(1)]],
    policyTenureMonths:          [12, Validators.required],
    occupationType:              ['', Validators.required],
    existingConditions:          ['None'],
    smokingStatus:               ['Non-Smoker', Validators.required],
    numberOfFamilyMembers:       [1,  [Validators.required, Validators.min(1)]],
    hospitalizationLast3Years:   ['No'],
    surgicalHistoryLast5Years:   ['No'],
    additionalNotes:             [''],
  });

  // ── PnC ─────────────────────────────────────────────────────────────
  readonly pncForm = this.fb.group({
    sumInsured:              [0,           [Validators.required, Validators.min(1)]],
    policyTenureMonths:      [12,          Validators.required],
    propertyType:            ['Residential', Validators.required],
    constructionYear:        [2000,        [Validators.required, Validators.min(1900), Validators.max(new Date().getFullYear())]],
    occupancyType:           ['Owner-Occupied', Validators.required],
    hasSecuritySystem:       ['Yes'],
    previousClaimsLast5Years:[0,           [Validators.required, Validators.min(0)]],
    additionalNotes:         [''],
  });

  // ── Commercial ──────────────────────────────────────────────────────
  readonly commercialForm = this.fb.group({
    sumInsured:         [0,       [Validators.required, Validators.min(1)]],
    policyTenureMonths: [12,      Validators.required],
    businessType:       ['',      Validators.required],
    numberOfEmployees:  [1,       [Validators.required, Validators.min(1)]],
    annualRevenue:      [0,       [Validators.required, Validators.min(0)]],
    yearsInOperation:   [1,       [Validators.required, Validators.min(0)]],
    riskLocation:       ['Urban', Validators.required],
    additionalNotes:    [''],
  });

  // ── Active form helper ───────────────────────────────────────────────
  readonly activeForm = computed(() => {
    switch (this.productLine()) {
      case 'Life':       return this.lifeForm;
      case 'Health':     return this.healthForm;
      case 'PnC':        return this.pncForm;
      case 'Commercial': return this.commercialForm;
      default:           return this.lifeForm;
    }
  });

  constructor(
    private svc: SubmissionApiService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.submissionId.set(id);
    if (!id) { this.loading.set(false); return; }

    // First fetch submission to get its productLine
    this.svc.getById(id).subscribe({
      next: (subRes: any) => {
        const sub = subRes?.data ?? subRes;
        const pl: ProductLine = sub?.productLine ?? 'Life';
        this.productLine.set(pl);

        // Then fetch existing questionnaire answers
        this.svc.getQuestionnaire(id).subscribe({
          next: (res: any) => {
            const q: Questionnaire = res?.data ?? res;
            this.questionnaire.set(q);
            if (q?.responsesJSON) {
              const r = q.responsesJSON as any;
              this.patchForm(pl, r);
            }
            this.loading.set(false);
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  private patchForm(pl: ProductLine, r: any) {
    switch (pl) {
      case 'Life':
        this.lifeForm.patchValue({
          sumInsured:           r.sumInsured           ?? 0,
          policyTenureMonths:   r.policyTenureMonths   ?? 12,
          occupationType:       r.occupationType       ?? '',
          smokingStatus:        r.smokingStatus        ?? 'Non-Smoker',
          annualIncome:         r.annualIncome         ?? 0,
          existingConditions:   r.existingConditions   ?? 'None',
          familyMedicalHistory: r.familyMedicalHistory ?? 'None',
          additionalNotes:      r.additionalNotes      ?? '',
        }); break;

      case 'Health':
        this.healthForm.patchValue({
          sumInsured:                r.sumInsured                ?? 0,
          policyTenureMonths:        r.policyTenureMonths        ?? 12,
          occupationType:            r.occupationType            ?? '',
          existingConditions:        r.existingConditions        ?? 'None',
          smokingStatus:             r.smokingStatus             ?? 'Non-Smoker',
          numberOfFamilyMembers:     r.numberOfFamilyMembers     ?? 1,
          hospitalizationLast3Years: r.hospitalizationLast3Years ?? 'No',
          surgicalHistoryLast5Years: r.surgicalHistoryLast5Years ?? 'No',
          additionalNotes:           r.additionalNotes           ?? '',
        }); break;

      case 'PnC':
        this.pncForm.patchValue({
          sumInsured:               r.sumInsured               ?? 0,
          policyTenureMonths:       r.policyTenureMonths       ?? 12,
          propertyType:             r.propertyType             ?? 'Residential',
          constructionYear:         r.constructionYear         ?? 2000,
          occupancyType:            r.occupancyType            ?? 'Owner-Occupied',
          hasSecuritySystem:        r.hasSecuritySystem        ?? 'Yes',
          previousClaimsLast5Years: r.previousClaimsLast5Years ?? 0,
          additionalNotes:          r.additionalNotes          ?? '',
        }); break;

      case 'Commercial':
        this.commercialForm.patchValue({
          sumInsured:         r.sumInsured         ?? 0,
          policyTenureMonths: r.policyTenureMonths ?? 12,
          businessType:       r.businessType       ?? '',
          numberOfEmployees:  r.numberOfEmployees  ?? 1,
          annualRevenue:      r.annualRevenue       ?? 0,
          yearsInOperation:   r.yearsInOperation   ?? 1,
          riskLocation:       r.riskLocation       ?? 'Urban',
          additionalNotes:    r.additionalNotes    ?? '',
        }); break;
    }
  }

  save() {
    const form = this.activeForm();
    if (form.invalid) { form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.svc.saveQuestionnaire(this.submissionId(), form.value).subscribe({
      next: () => { this.saving.set(false); this.flash('success', 'Questionnaire saved successfully.'); },
      error: () => { this.saving.set(false); this.flash('danger', 'Save failed. Please try again.'); },
    });
  }

  productLineLabel(): string {
    const map: Record<string, string> = {
      Life: 'Life Insurance', Health: 'Health Insurance',
      PnC: 'Property & Casualty (PnC)', Commercial: 'Commercial Insurance',
    };
    return map[this.productLine() ?? ''] ?? 'Insurance';
  }

  productLineBadgeClass(): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[this.productLine() ?? ''] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
