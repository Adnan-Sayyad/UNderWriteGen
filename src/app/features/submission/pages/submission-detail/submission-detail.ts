import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { SubmissionApiService } from '../../services/submission-api.service';
import { Submission, Attachment, CompletenessCheck, Questionnaire, SubmissionStatus, DocType, Subjectivity, UWNote } from '../../models/submission.model';
import { AuthService } from '../../../../core/auth/auth.service';

// Which statuses each role can transition TO from a given current status
const ROLE_TRANSITIONS: Record<string, Record<string, SubmissionStatus[]>> = {
  Agent:         { Draft:        ['IntakeComplete'] },
  UWAssistant:   { IntakeComplete: ['UnderReview'] },
  Underwriter:   { IntakeComplete: ['UnderReview'],
                   UnderReview:    ['Quoted', 'Declined'] },
  PricingAnalyst:{ UnderReview:   ['Quoted'] },
  Operations:    { Quoted:        ['Expired'], Declined: ['Expired'] },
  Compliance:    {},   // read-only
  Admin:         { Draft:        ['IntakeComplete','UnderReview','Quoted','Declined','Expired'],
                   IntakeComplete:['Draft','UnderReview','Quoted','Declined','Expired'],
                   UnderReview:  ['Draft','IntakeComplete','Quoted','Declined','Expired'],
                   Quoted:       ['Draft','IntakeComplete','UnderReview','Declined','Expired'],
                   Declined:     ['Draft','IntakeComplete','UnderReview','Quoted','Expired'],
                   Expired:      ['Draft','IntakeComplete','UnderReview','Quoted','Declined'] },
};

type TabId = 'overview' | 'questionnaire' | 'attachments' | 'completeness' | 'subjectivities' | 'notes';

const ALL_STATUSES: SubmissionStatus[] = ['Draft', 'IntakeComplete', 'UnderReview', 'Quoted', 'Declined', 'Expired'];
const DOC_TYPES: DocType[] = ['KYC', 'Financial', 'Medical', 'Inspection', 'Photos'];
const ALLOWED_MIME = [
  'image/jpeg',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
];
const ALLOWED_EXT = ['.jpg', '.jpeg', '.pdf', '.doc', '.docx'];

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

  private readonly auth        = inject(AuthService);

  // Statuses this user can change TO from the current submission status
  readonly allowedTransitions  = computed<SubmissionStatus[]>(() => {
    const role    = this.auth.currentUser()?.role ?? '';
    const current = this.submission()?.status ?? '';
    return ROLE_TRANSITIONS[role]?.[current] ?? [];
  });

  readonly canChangeStatus     = computed(() => this.allowedTransitions().length > 0);
  readonly docTypes            = DOC_TYPES;

  readonly selectedDocType     = signal<DocType>('KYC');
  readonly selectedFile        = signal<File | null>(null);
  readonly fileError           = signal<string | null>(null);
  readonly uploading           = signal(false);
  readonly deletingId          = signal<string | null>(null);

  readonly loadingCompleteness = signal(false);
  readonly runningCheck        = signal(false);

  // Subjectivities
  readonly subjectivities      = signal<Subjectivity[]>([]);
  readonly loadingSubj         = signal(false);
  readonly newSubjDesc         = signal('');
  readonly newSubjDueDate      = signal('');
  readonly creatingSubj        = signal(false);

  // UW Notes
  readonly notes               = signal<UWNote[]>([]);
  readonly loadingNotes        = signal(false);
  readonly newNoteText         = signal('');
  readonly addingNote          = signal(false);
  readonly deletingNoteId      = signal<string | null>(null);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Submissions', route: '/submissions' },
    { label: 'Detail' },
  ];

  private submissionId = '';

  constructor(
    private svc: SubmissionApiService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  logBreach(): void {
    this.router.navigate(['/compliance/authority-breaches'],
      { queryParams: { submissionId: this.submissionId } });
  }

  logException(): void {
    this.router.navigate(['/compliance/exceptions'],
      { queryParams: { submissionId: this.submissionId } });
  }

  newChecklist(): void {
    this.router.navigate(['/compliance/checklists'],
      { queryParams: { submissionId: this.submissionId } });
  }

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
    if (tab === 'attachments'     && !this.attachments().length)     this.loadAttachments();
    if (tab === 'completeness'    && !this.completeness())           this.loadCompleteness();
    if (tab === 'questionnaire'   && !this.questionnaire())          this.loadQuestionnaire();
    if (tab === 'subjectivities'  && !this.subjectivities().length)  this.loadSubjectivities();
    if (tab === 'notes'           && !this.notes().length)           this.loadNotes();
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

  updateStatus(newStatus: string, selectEl?: EventTarget | null): void {
    if (!newStatus || newStatus === this.submission()?.status) return;
    this.updatingStatus.set(true);
    this.svc.updateStatus(this.submissionId, newStatus).subscribe({
      next: res => {
        const d: any = res;
        const updated = d?.data ?? d;
        this.submission.update(s => s ? { ...s, status: updated?.status ?? newStatus as SubmissionStatus } : s);
        this.updatingStatus.set(false);
        // Reset the dropdown back to placeholder
        if (selectEl) (selectEl as HTMLSelectElement).value = '';
        this.flash('success', `Status changed to ${newStatus}`);
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
    if (!this.questionnaire()) {
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
          this.doRunCheck();
        },
        error: () => this.doRunCheck(),
      });
    } else {
      this.doRunCheck();
    }
  }

  private doRunCheck(): void {
    const missing = this.buildMissingItems();
    this.svc.runCompletenessCheck(this.submissionId, missing).subscribe({
      next: res => {
        const d: any = res;
        this.completeness.set(d?.data ?? d);
        this.runningCheck.set(false);
        this.flash(
          missing.length === 0 ? 'success' : 'danger',
          missing.length === 0 ? 'All items complete!' : `${missing.length} missing item(s) found.`,
        );
      },
      error: () => { this.runningCheck.set(false); this.flash('danger', 'Check failed'); },
    });
  }

  private buildMissingItems(): string[] {
    const missing: string[] = [];

    // Questionnaire — all fields mandatory
    const q = this.questionnaire();
    if (!q) {
      missing.push('Questionnaire has not been filled');
    } else {
      const r: any = q.responsesJSON;
      if (!r?.occupationType)                      missing.push('Questionnaire: Occupation Type is required');
      if (!r?.sumInsured || r.sumInsured <= 0)      missing.push('Questionnaire: Sum Insured is required');
      if (!r?.existingConditions)                  missing.push('Questionnaire: Existing Conditions is required');
      if (!r?.smokingStatus)                       missing.push('Questionnaire: Smoking Status is required');
      if (!r?.annualIncome || r.annualIncome <= 0) missing.push('Questionnaire: Annual Income is required');
    }

    // Attachments — any 2 documents enough
    const attachmentCount = this.attachments().length;
    if (attachmentCount < 2) {
      missing.push(`At least 2 attachments required (uploaded: ${attachmentCount})`);
    }

    return missing;
  }

  // ── Subjectivities ─────────────────────────────────────────────────────────

  private loadSubjectivities(): void {
    this.loadingSubj.set(true);
    this.svc.getSubjectivities(this.submissionId).subscribe({
      next: res => { this.subjectivities.set(res?.data ?? []); this.loadingSubj.set(false); },
      error: () => this.loadingSubj.set(false),
    });
  }

  createSubjectivity(): void {
    if (!this.newSubjDesc().trim() || !this.newSubjDueDate()) return;
    this.creatingSubj.set(true);
    this.svc.createSubjectivity(this.submissionId, this.newSubjDesc(), this.newSubjDueDate()).subscribe({
      next: res => {
        const s = res?.data;
        if (s) this.subjectivities.update(list => [s, ...list]);
        this.newSubjDesc.set('');
        this.newSubjDueDate.set('');
        this.creatingSubj.set(false);
        this.flash('success', 'Subjectivity created!');
      },
      error: () => { this.creatingSubj.set(false); this.flash('danger', 'Create failed.'); },
    });
  }

  updateSubjectivityStatus(sub: Subjectivity, status: string): void {
    this.svc.updateSubjectivityStatus(sub.subjectivityId, status).subscribe({
      next: () => {
        this.subjectivities.update(list =>
          list.map(s => s.subjectivityId === sub.subjectivityId ? { ...s, status: status as any } : s)
        );
        this.flash('success', `Marked as ${status}`);
      },
      error: () => this.flash('danger', 'Update failed.'),
    });
  }

  subjStatusClass(s: string): string {
    return s === 'Met' ? 'bg-success' : s === 'Waived' ? 'bg-secondary' : 'bg-warning text-dark';
  }

  // ── UW Notes ───────────────────────────────────────────────────────────────

  private loadNotes(): void {
    this.loadingNotes.set(true);
    this.svc.getNotes(this.submissionId).subscribe({
      next: res => { this.notes.set(res?.data ?? []); this.loadingNotes.set(false); },
      error: () => this.loadingNotes.set(false),
    });
  }

  addNote(): void {
    if (!this.newNoteText().trim()) return;
    this.addingNote.set(true);
    this.svc.addNote(this.submissionId, this.newNoteText()).subscribe({
      next: res => {
        const n = res?.data;
        if (n) this.notes.update(list => [n, ...list]);
        this.newNoteText.set('');
        this.addingNote.set(false);
        this.flash('success', 'Note added!');
      },
      error: () => { this.addingNote.set(false); this.flash('danger', 'Failed to add note.'); },
    });
  }

  deleteNote(noteId: string): void {
    this.deletingNoteId.set(noteId);
    this.svc.deleteNote(noteId).subscribe({
      next: () => {
        this.notes.update(list => list.filter(n => n.noteId !== noteId));
        this.deletingNoteId.set(null);
        this.flash('success', 'Note deleted.');
      },
      error: () => { this.deletingNoteId.set(null); this.flash('danger', 'Delete failed.'); },
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
