import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';

import { UnderwritingApiService } from '../../services/underwriting-api.service';
import { UWDecision, UWNote, DecisionType } from '../../models/underwriting.model';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';

const DECISIONS: { value: DecisionType; label: string; css: string; outlineCss: string }[] = [
  { value: 'Approve',  label: 'Approve',      css: 'btn-success',          outlineCss: 'btn-outline-success' },
  { value: 'Decline',  label: 'Decline',      css: 'btn-danger',           outlineCss: 'btn-outline-danger' },
  { value: 'Refer',    label: 'Refer to UW',  css: 'btn-warning',          outlineCss: 'btn-outline-warning' },
  { value: 'MoreInfo', label: 'Request Info', css: 'btn-outline-secondary', outlineCss: 'btn-outline-secondary' },
];

@Component({
  selector: 'app-uw-decision',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './uw-decision.html',
  styleUrl: './uw-decision.css',
})
export class UwDecisionPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly existingDecision  = signal<UWDecision | null>(null);
  readonly notes             = signal<UWNote[]>([]);
  readonly loading           = signal(false);
  readonly saving            = signal(false);
  readonly addingNote        = signal(false);
  readonly deletingNoteId    = signal<string | null>(null);
  readonly submissionId      = signal('');
  readonly alertMsg          = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly showOverwriteWarn = signal(false);
  readonly decisions         = DECISIONS;

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'UW Workbench', route: '/underwriting/workbench' },
    { label: 'Decision' },
  ];

  readonly decisionForm = this.fb.group({
    decision: ['Approve' as DecisionType, Validators.required],
    reason:   ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly noteForm = this.fb.group({
    noteText: ['', [Validators.required, Validators.minLength(5)]],
  });

  constructor(
    private svc: UnderwritingApiService,
    private subSvc: SubmissionApiService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('submissionId') ?? '';
    this.submissionId.set(id);
    if (id) { this.loadDecision(id); this.loadNotes(id); }
  }

  private loadDecision(id: string): void {
    this.svc.getDecision(id).subscribe({
      next: (res: any) => {
        const d = res?.data ?? null;
        this.existingDecision.set(d);
        if (d) {
          this.decisionForm.patchValue({ decision: d.decision, reason: d.reason });
        }
      },
      error: () => {},
    });
  }

  private loadNotes(id: string): void {
    this.svc.getNotes(id).subscribe({
      next: (res: any) => this.notes.set(res?.data ?? []),
      error: () => {},
    });
  }

  requestSubmit(): void {
    if (this.decisionForm.invalid) { this.decisionForm.markAllAsTouched(); return; }
    if (this.existingDecision()) {
      this.showOverwriteWarn.set(true);
    } else {
      this.doSubmit();
    }
  }

  cancelOverwrite(): void { this.showOverwriteWarn.set(false); }

  confirmOverwrite(): void {
    this.showOverwriteWarn.set(false);
    this.doSubmit();
  }

  private doSubmit(): void {
    this.saving.set(true);
    const v = this.decisionForm.value;
    const decision = v.decision as DecisionType;
    this.svc.submitDecision(this.submissionId(), {
      decision,
      reason: v.reason!,
    }).subscribe({
      next: (res: any) => {
        this.existingDecision.set(res?.data ?? null);
        this.saving.set(false);
        if (decision === 'Approve') {
          // Auto-update submission status to Quoted
          this.subSvc.updateStatus(this.submissionId(), 'Quoted').subscribe({
            next: () => {
              this.flash('success', '✅ Approved! Submission status updated to Quoted.');
              setTimeout(() => this.router.navigate(['/underwriting/workbench']), 1800);
            },
            error: () => {
              // Even if status update fails, decision was saved
              this.flash('success', '✅ Decision approved. (Status update failed — update manually.)');
              setTimeout(() => this.router.navigate(['/underwriting/workbench']), 2000);
            },
          });
        } else {
          this.flash('success', 'Decision submitted.');
          setTimeout(() => this.router.navigate(['/underwriting/workbench']), 1500);
        }
      },
      error: (err: any) => {
        this.saving.set(false);
        this.flash('danger', err?.error?.message ?? 'Submit failed.');
      },
    });
  }

  addNote(): void {
    if (this.noteForm.invalid) { this.noteForm.markAllAsTouched(); return; }
    this.addingNote.set(true);
    this.svc.addNote(this.submissionId(), { noteText: this.noteForm.value.noteText! }).subscribe({
      next: () => {
        this.noteForm.reset();
        this.addingNote.set(false);
        this.loadNotes(this.submissionId());
      },
      error: (err: any) => {
        this.addingNote.set(false);
        this.flash('danger', err?.error?.message ?? 'Note save failed.');
      },
    });
  }

  deleteNote(noteId: string): void {
    this.deletingNoteId.set(noteId);
    this.svc.deleteNote(this.submissionId(), noteId).subscribe({
      next: () => {
        this.deletingNoteId.set(null);
        this.notes.update(list => list.filter(n => n.noteId !== noteId));
      },
      error: () => { this.deletingNoteId.set(null); this.flash('danger', 'Failed to delete note.'); },
    });
  }

  decisionClass(d: string): string {
    const map: Record<string, string> = {
      Approve: 'bg-success', Decline: 'bg-danger',
      Refer: 'bg-warning text-dark', MoreInfo: 'bg-secondary',
    };
    return map[d] ?? 'bg-secondary';
  }

  shortId(id: string): string {
    return id ? id.slice(0, 8) + '…' : '—';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
