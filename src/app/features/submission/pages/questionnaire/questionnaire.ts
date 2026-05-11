import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Questionnaire } from '../../models/submission.model';

@Component({
  selector: 'app-questionnaire',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './questionnaire.html',
  styleUrl: './questionnaire.css',
})
export class QuestionnairePage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly submissionId = signal('');
  readonly questionnaire = signal<Questionnaire | null>(null);
  readonly loading  = signal(true);
  readonly saving   = signal(false);
  readonly alertMsg = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'Questionnaire' },
  ];

  readonly form = this.fb.group({
    occupationType:      ['', Validators.required],
    sumInsured:          [0, [Validators.required, Validators.min(0)]],
    existingConditions:  ['None'],
    smokingStatus:       ['Non-Smoker'],
    annualIncome:        [0],
    additionalNotes:     [''],
  });

  constructor(
    private svc: SubmissionApiService,
    private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.submissionId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.svc.getQuestionnaire(id).subscribe({
      next: (res: any) => {
        const q: Questionnaire = res?.data ?? res;
        this.questionnaire.set(q);
        if (q?.responsesJSON) {
          const r = q.responsesJSON as any;
          this.form.patchValue({
            occupationType:     r.occupationType     ?? '',
            sumInsured:         r.sumInsured         ?? 0,
            existingConditions: r.existingConditions ?? 'None',
            smokingStatus:      r.smokingStatus      ?? 'Non-Smoker',
            annualIncome:       r.annualIncome       ?? 0,
            additionalNotes:    r.additionalNotes    ?? '',
          });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  save() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.value;
    this.saving.set(true);
    this.svc.saveQuestionnaire(this.submissionId(), {
      occupationType:     v.occupationType,
      sumInsured:         v.sumInsured,
      existingConditions: v.existingConditions,
      smokingStatus:      v.smokingStatus,
      annualIncome:       v.annualIncome,
      additionalNotes:    v.additionalNotes,
    }).subscribe({
      next: () => { this.saving.set(false); this.flash('success', 'Questionnaire saved.'); },
      error: () => { this.saving.set(false); this.flash('danger', 'Save failed.'); },
    });
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
