import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, Attachment, CompletenessCheck, Questionnaire, SubmissionStatus, DocType } from '../../models/submission.model';

type TabId = 'overview' | 'questionnaire' | 'attachments' | 'completeness';

const ALL_STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];
const DOC_TYPES: DocType[] = ['KYC', 'Financial', 'Medical', 'Inspection', 'Photos'];

@Component({
  selector: 'app-submission-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState],
  templateUrl: './submission-detail.html',
  styleUrl: './submission-detail.css',
})
export class SubmissionDetailPage implements OnInit {
  readonly submission          = signal<Submission | null>(null);
  readonly attachments         = signal<Attachment[]>([]);
  readonly completeness        = signal<CompletenessCheck | null>(null);
  readonly questionnaire       = signal<Questionnaire | null>(null);
  readonly loading             = signal(false);
  readonly activeTab           = signal<TabId>('overview');

  // Questionnaire form fields
  readonly qOccupationType     = signal('');
  readonly qSumInsured         = signal<number>(0);
  readonly qExistingConditions = signal('None');
  readonly qSmokingStatus      = signal('Non-Smoker');
  readonly qAnnualIncome       = signal<number>(0);
  readonly qAdditionalNotes    = signal('');
  readonly savingQuestionnaire = signal(false);
  readonly alertMsg            = signal<{ type: 'success' | 'danger'; text: string } | null>(null);

  readonly updatingStatus      = signal(false);
  readonly allStatuses         = ALL_STATUSES;
  readonly docTypes            = DOC_TYPES;

  readonly selectedDocType     = signal<DocType>('KYC');
  readonly selectedFile        = signal<File | null>(null);
  readonly uploading           = signal(false);
  readonly deletingId          = signal<string | null>(null);

  readonly loadingCompleteness = signal(false);
  readonly runningCheck        = signal(false);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'Detail' },
  ];

  private submissionId = '';

  constructor(private svc: SubmissionApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.submissionId = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.svc.getById(this.submissionId).subscribe({
      next: res => {
        this.submission.set(res.data ?? null);
        this.loading.set(false);
        this.loadAttachments();
      },
      error: () => this.loading.set(false),
    });
  }

  setTab(tab: TabId): void {
    this.activeTab.set(tab);
    if (tab === 'attachments'   && !this.attachments().length)  this.loadAttachments();
    if (tab === 'completeness'  && !this.completeness())        this.loadCompleteness();
    if (tab === 'questionnaire' && !this.questionnaire())       this.loadQuestionnaire();
  }

  private loadQuestionnaire(): void {
    this.svc.getQuestionnaire(this.submissionId).subscribe({
      next: res => {
        const q = res?.data ?? null;
        this.questionnaire.set(q);
        if (q?.responsesJSON) {
          const r: any = q.responsesJSON;
          this.qOccupationType.set(r.occupationType ?? '');
          this.qSumInsured.set(r.sumInsured ?? 0);
          this.qExistingConditions.set(r.existingConditions ?? 'None');
          this.qSmokingStatus.set(r.smokingStatus ?? 'Non-Smoker');
          this.qAnnualIncome.set(r.annualIncome ?? 0);
          this.qAdditionalNotes.set(r.additionalNotes ?? '');
        }
      },
      error: () => {},
    });
  }

  saveQuestionnaire(): void {
    this.savingQuestionnaire.set(true);
    this.svc.saveQuestionnaire(this.submissionId, {
      occupationType:     this.qOccupationType(),
      sumInsured:         this.qSumInsured(),
      existingConditions: this.qExistingConditions(),
      smokingStatus:      this.qSmokingStatus(),
      annualIncome:       this.qAnnualIncome(),
      additionalNotes:    this.qAdditionalNotes(),
    }).subscribe({
      next: () => { this.savingQuestionnaire.set(false); this.flash('success', 'Questionnaire saved successfully!'); },
      error: () => { this.savingQuestionnaire.set(false); this.flash('danger', 'Failed to save questionnaire'); },
    });
  }

  // ── Status Update ──────────────────────────────────────────────────────────

  updateStatus(newStatus: string): void {
    if (!newStatus || newStatus === this.submission()?.status) return;
    this.updatingStatus.set(true);
    this.svc.updateStatus(this.submissionId, newStatus).subscribe({
      next: res => {
        const d: any = res;
        const updated = d?.data ?? d;
        this.submission.update(s => s ? { ...s, status: updated?.status ?? newStatus as SubmissionStatus } : s);
        this.updatingStatus.set(false);
        this.flash('success', `Status updated to ${newStatus}`);
      },
      error: () => { this.updatingStatus.set(false); this.flash('danger', 'Failed to update status'); },
    });
  }

  // ── Attachments ────────────────────────────────────────────────────────────

  private loadAttachments(): void {
    this.svc.getAttachments(this.submissionId).subscribe({
      next: res => { const d: any = res; this.attachments.set(d?.data ?? []); },
      error: () => {},
    });
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  uploadAttachment(): void {
    const file = this.selectedFile();
    if (!file) return;
    this.uploading.set(true);
    this.svc.uploadAttachment(this.submissionId, this.selectedDocType(), file).subscribe({
      next: res => {
        const d: any = res;
        const att: Attachment = d?.data ?? d;
        this.attachments.update(list => [att, ...list]);
        this.selectedFile.set(null);
        this.uploading.set(false);
        this.flash('success', 'Document uploaded');
      },
      error: () => { this.uploading.set(false); this.flash('danger', 'Upload failed'); },
    });
  }

  deleteAttachment(attachmentId: string): void {
    this.deletingId.set(attachmentId);
    this.svc.deleteAttachment(this.submissionId, attachmentId).subscribe({
      next: () => {
        this.attachments.update(list => list.filter(a => a.attachmentId !== attachmentId));
        this.deletingId.set(null);
        this.flash('success', 'Attachment removed');
      },
      error: () => { this.deletingId.set(null); this.flash('danger', 'Delete failed'); },
    });
  }

  // ── Completeness ───────────────────────────────────────────────────────────

  private loadCompleteness(): void {
    this.loadingCompleteness.set(true);
    this.svc.getCompletenessCheck(this.submissionId).subscribe({
      next: res => { const d: any = res; this.completeness.set(d?.data ?? d); this.loadingCompleteness.set(false); },
      error: () => this.loadingCompleteness.set(false),
    });
  }

  runCompletenessCheck(): void {
    this.runningCheck.set(true);
    this.svc.runCompletenessCheck(this.submissionId).subscribe({
      next: res => {
        const d: any = res;
        this.completeness.set(d?.data ?? d);
        this.runningCheck.set(false);
        this.flash('success', 'Completeness check complete');
      },
      error: () => { this.runningCheck.set(false); this.flash('danger', 'Check failed'); },
    });
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark',
      UnderReview: 'bg-warning text-dark', Quoted: 'bg-primary',
      Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[status] ?? 'bg-secondary';
  }

  completenessClass(status: string): string {
    return status === 'Complete' ? 'text-success' : 'text-warning';
  }

  flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 3500);
  }
}
