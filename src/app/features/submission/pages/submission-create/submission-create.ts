import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SubmissionApiService } from '../../services/submission-api.service';
import { ProductLine, DocType, Attachment, CompletenessCheck } from '../../models/submission.model';

const PRODUCT_LINES: ProductLine[] = ['Life', 'Health', 'PnC', 'Commercial'];
const DOC_TYPES: DocType[] = ['KYC', 'Financial', 'Medical', 'Inspection', 'Photos'];
const ALLOWED_MIME = [
  'image/jpeg',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
];
const ALLOWED_EXT = ['.jpg', '.jpeg', '.pdf', '.doc', '.docx'];

@Component({
  selector: 'app-submission-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './submission-create.html',
  styleUrl: './submission-create.css',
})
export class SubmissionCreatePage {
  private readonly fb = inject(FormBuilder);

  readonly step         = signal<1 | 2 | 3 | 4>(1);
  readonly saving       = signal(false);
  readonly alertMsg     = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly productLines = PRODUCT_LINES;
  readonly docTypes     = DOC_TYPES;

  submissionId = '';

  // ── Questionnaire ──────────────────────────────────────────────────────────
  readonly qOccupationType     = signal('');
  readonly qSumInsured         = signal<number>(0);
  readonly qExistingConditions = signal('None');
  readonly qSmokingStatus      = signal('Non-Smoker');
  readonly qAnnualIncome       = signal<number>(0);
  readonly qAdditionalNotes    = signal('');
  readonly qErrors             = signal<Record<string, string>>({});
  readonly qSaved              = signal(false);

  // ── Attachments ────────────────────────────────────────────────────────────
  readonly attachments     = signal<Attachment[]>([]);
  readonly selectedDocType = signal<DocType>('KYC');
  readonly selectedFile    = signal<File | null>(null);
  readonly fileError       = signal<string | null>(null);
  readonly uploading       = signal(false);

  // ── Completeness ───────────────────────────────────────────────────────────
  readonly completeness  = signal<CompletenessCheck | null>(null);
  readonly runningCheck  = signal(false);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'New Submission' },
  ];

  readonly form = this.fb.group({
    partyId:       ['', Validators.required],
    agentId:       ['', Validators.required],
    productLine:   ['Life' as ProductLine, Validators.required],
    inceptionDate: ['', Validators.required],
  });

  constructor(
    private svc: SubmissionApiService,
    private router: Router,
  ) {}

  // ── Step 1: Create Submission ──────────────────────────────────────────────

  createSubmission(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.value;
    this.svc.create({
      partyId:       v.partyId!,
      agentId:       v.agentId!,
      productLine:   v.productLine as ProductLine,
      inceptionDate: v.inceptionDate!,
      coverageJSON:  {},
    }).subscribe({
      next: (res: any) => {
        const sub = res?.data ?? res;
        this.submissionId = sub?.submissionId ?? sub?.submissionID ?? '';
        this.saving.set(false);
        this.step.set(2);
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Create failed.'); },
    });
  }

  // ── Step 2: Questionnaire ──────────────────────────────────────────────────

  saveQuestionnaire(): void {
    const errors: Record<string, string> = {};
    if (!this.qOccupationType().trim())                        errors['occupationType']     = 'Occupation type is required.';
    if (!this.qSumInsured() || this.qSumInsured() <= 0)        errors['sumInsured']         = 'Sum insured must be greater than 0.';
    if (!this.qExistingConditions().trim())                    errors['existingConditions'] = 'Existing conditions is required.';
    if (!this.qSmokingStatus().trim())                         errors['smokingStatus']      = 'Smoking status is required.';
    if (!this.qAnnualIncome() || this.qAnnualIncome() <= 0)    errors['annualIncome']       = 'Annual income must be greater than 0.';
    this.qErrors.set(errors);
    if (Object.keys(errors).length) return;

    this.saving.set(true);
    this.svc.saveQuestionnaire(this.submissionId, {
      occupationType:     this.qOccupationType(),
      sumInsured:         this.qSumInsured(),
      existingConditions: this.qExistingConditions(),
      smokingStatus:      this.qSmokingStatus(),
      annualIncome:       this.qAnnualIncome(),
      additionalNotes:    this.qAdditionalNotes(),
    }).subscribe({
      next: () => { this.saving.set(false); this.qSaved.set(true); this.step.set(3); },
      error: () => { this.saving.set(false); this.flash('danger', 'Failed to save questionnaire.'); },
    });
  }

  skipQuestionnaire(): void {
    this.step.set(3);
  }

  // ── Step 3: Attachments ────────────────────────────────────────────────────

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file  = input.files?.[0] ?? null;
    this.fileError.set(null);
    if (file && !this.isValidFile(file)) {
      this.fileError.set('Only JPG, PDF, DOC, DOCX files are allowed.');
      this.selectedFile.set(null);
      input.value = '';
      return;
    }
    this.selectedFile.set(file);
  }

  private isValidFile(file: File): boolean {
    const ext = '.' + (file.name.split('.').pop()?.toLowerCase() ?? '');
    return ALLOWED_MIME.includes(file.type) || ALLOWED_EXT.includes(ext);
  }

  uploadAttachment(): void {
    const file = this.selectedFile();
    if (!file) return;
    this.uploading.set(true);
    this.svc.uploadAttachment(this.submissionId, this.selectedDocType(), file).subscribe({
      next: (res: any) => {
        const att: Attachment = res?.data ?? res;
        this.attachments.update(list => [att, ...list]);
        this.selectedFile.set(null);
        this.uploading.set(false);
        this.flash('success', `${this.selectedDocType()} uploaded successfully.`);
      },
      error: () => { this.uploading.set(false); this.flash('danger', 'Upload failed.'); },
    });
  }

  proceedToCheck(): void {
    this.step.set(4);
    this.runCheck();
  }

  // ── Step 4: Completeness Check ─────────────────────────────────────────────

  runCheck(): void {
    this.runningCheck.set(true);
    const missing = this.buildMissingItems();
    this.svc.runCompletenessCheck(this.submissionId, missing).subscribe({
      next: (res: any) => {
        this.completeness.set(res?.data ?? res);
        this.runningCheck.set(false);
      },
      error: () => { this.runningCheck.set(false); this.flash('danger', 'Completeness check failed.'); },
    });
  }

  private buildMissingItems(): string[] {
    const missing: string[] = [];

    // Questionnaire — all fields mandatory
    if (!this.qSaved()) {
      missing.push('Questionnaire has not been filled');
    } else {
      if (!this.qOccupationType().trim())                      missing.push('Questionnaire: Occupation Type is required');
      if (!this.qSumInsured() || this.qSumInsured() <= 0)      missing.push('Questionnaire: Sum Insured is required');
      if (!this.qExistingConditions().trim())                  missing.push('Questionnaire: Existing Conditions is required');
      if (!this.qSmokingStatus().trim())                       missing.push('Questionnaire: Smoking Status is required');
      if (!this.qAnnualIncome() || this.qAnnualIncome() <= 0)  missing.push('Questionnaire: Annual Income is required');
    }

    // Attachments — any 2 documents enough
    const attachmentCount = this.attachments().length;
    if (attachmentCount < 2) {
      missing.push(`At least 2 attachments required (uploaded: ${attachmentCount})`);
    }

    return missing;
  }

  finish(): void {
    this.router.navigate(['/submissions', this.submissionId]);
  }

  finishToList(): void {
    this.router.navigate(['/submissions']);
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
