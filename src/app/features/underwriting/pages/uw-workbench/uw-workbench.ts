import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import {
  Submission, Subjectivity, UWNote, SubmissionStatus,
  Attachment, Questionnaire, CompletenessCheck, RiskScore
} from '../../../submission/models/submission.model';

const ROLE_TRANSITIONS: Record<string, Record<string, SubmissionStatus[]>> = {
  UWAssistant:   { IntakeComplete: ['UnderReview'] },
  Underwriter:   { IntakeComplete: ['UnderReview'] },   // Approve/Decline via UW Decision panel
  Admin:         {
    IntakeComplete: ['UnderReview', 'Approved', 'Declined'],
    UnderReview:    ['IntakeComplete', 'Approved', 'Declined'],
    Approved:       ['UnderReview', 'Quoted', 'Declined'],
    Quoted:         ['Approved', 'PolicyBound', 'Declined'],
    PolicyBound:    ['Issued'],
  },
};

type DetailTab = 'overview' | 'questionnaire' | 'attachments' | 'subjectivities' | 'notes' | 'rules';

@Component({
  selector: 'app-uw-workbench',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './uw-workbench.html',
  styleUrl: './uw-workbench.css',
})
export class UwWorkbenchPage implements OnInit {

  // ── Submission List ────────────────────────────────────────────────────────
  readonly submissions   = signal<Submission[]>([]);
  readonly loading       = signal(false);
  readonly searchQuery   = signal('');
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);
  readonly alertMsg      = signal<{ type: 'success'|'danger'; text: string }|null>(null);

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    return this.submissions().filter(s =>
      !q || s.submissionId.toLowerCase().includes(q) || s.productLine.toLowerCase().includes(q)
    );
  });

  // ── Detail Panel ───────────────────────────────────────────────────────────
  readonly selected       = signal<Submission | null>(null);
  readonly detailTab      = signal<DetailTab>('overview');
  readonly detailLoading  = signal(false);

  readonly questionnaire  = signal<Questionnaire | null>(null);
  readonly attachments    = signal<Attachment[]>([]);
  readonly completeness   = signal<CompletenessCheck | null>(null);
  readonly subjectivities = signal<Subjectivity[]>([]);
  readonly notes          = signal<UWNote[]>([]);
  readonly riskScore      = signal<RiskScore | null>(null);
  readonly calculatingRisk = signal(false);
  readonly subjCountMap   = signal<Record<string, number>>({});

  // Subjectivity create
  readonly newSubjDesc    = signal('');
  readonly newSubjDue     = signal('');
  readonly creatingSubj   = signal(false);

  // Note create
  readonly newNoteText    = signal('');
  readonly addingNote     = signal(false);

  // Rules evaluation
  readonly rulesResult      = signal<any | null>(null);
  readonly evaluatingRules  = signal(false);

  // UW Decision form
  readonly decisionValue    = signal('');
  readonly decisionReason   = signal('');
  readonly submittingDecision = signal(false);

  // Status change
  readonly updatingStatus = signal(false);
  private readonly auth   = inject(AuthService);

  readonly allowedTransitions = computed<SubmissionStatus[]>(() => {
    const role    = this.auth.currentUser()?.role ?? '';
    const current = this.selected()?.status ?? '';
    return ROLE_TRANSITIONS[role]?.[current] ?? [];
  });

  readonly canChangeStatus = computed(() => this.allowedTransitions().length > 0);

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Underwriting', route: '/underwriting' },
    { label: 'Workbench' },
  ];

  constructor(private svc: SubmissionApiService) {}

  ngOnInit(): void { this.load(); }

  // ── List ───────────────────────────────────────────────────────────────────

  load(page = this.currentPage()): void {
    this.loading.set(true);
    // Load all submissions at once so the client-side status filter (IntakeComplete+)
    // works across the full dataset, not just a single page.
    this.svc.getAll({ page: 0, size: 1000, sort: 'createdDate', direction: 'desc' } as any).subscribe({
      next: res => {
        const all = res.content ?? [];
        const uw  = all
          .filter((s: any) => ['IntakeComplete','UnderReview','Approved','Quoted','PolicyBound'].includes(s.status))
          .sort((a: any, b: any) =>
            new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime());
        this.submissions.set(uw);
        this.totalPages.set(Math.max(1, Math.ceil(uw.length / this.pageSize())));
        this.totalElements.set(uw.length);
        this.currentPage.set(0);
        this.loading.set(false);
        // Load open subjectivity counts for all submissions
        this.svc.getAllSubjectivities().subscribe({
          next: r => {
            const map: Record<string, number> = {};
            (r?.data ?? []).filter((s: any) => s.status === 'Open')
              .forEach((s: any) => { map[s.submissionId] = (map[s.submissionId] ?? 0) + 1; });
            this.subjCountMap.set(map);
          },
          error: () => {},
        });
      },
      error: () => this.loading.set(false),
    });
  }

  // ── Select Submission → Load Details ──────────────────────────────────────

  selectSubmission(sub: Submission): void {
    if (this.selected()?.submissionId === sub.submissionId) {
      this.selected.set(null); // toggle close
      return;
    }
    this.selected.set(sub);
    this.detailTab.set('overview');
    this.questionnaire.set(null);
    this.attachments.set([]);
    this.completeness.set(null);
    this.subjectivities.set([]);
    this.notes.set([]);
    this.riskScore.set(null);
    this.rulesResult.set(null);
    this.decisionValue.set('');
    this.decisionReason.set('');
    this.loadDetailData(sub.submissionId);
  }

  private loadDetailData(id: string): void {
    this.detailLoading.set(true);
    // Load questionnaire
    this.svc.getQuestionnaire(id).subscribe({
      next: res => this.questionnaire.set(res?.data ?? null),
      error: () => {},
    });
    // Load attachments
    this.svc.getAttachments(id).subscribe({
      next: res => { const d: any = res; this.attachments.set(d?.data ?? []); },
      error: () => {},
    });
    // Load completeness
    this.svc.getCompletenessCheck(id).subscribe({
      next: res => { const d: any = res; this.completeness.set(d?.data ?? null); this.detailLoading.set(false); },
      error: () => this.detailLoading.set(false),
    });
    // Load subjectivities
    this.svc.getSubjectivities(id).subscribe({
      next: res => this.subjectivities.set(res?.data ?? []),
      error: () => {},
    });
    // Load notes
    this.svc.getNotes(id).subscribe({
      next: res => this.notes.set(res?.data ?? []),
      error: () => {},
    });
    // Load risk score
    this.svc.getRiskScore(id).subscribe({
      next: res => this.riskScore.set(res?.data ?? null),
      error: () => this.riskScore.set(null),
    });
  }

  updateStatus(newStatus: string, el?: EventTarget | null): void {
    const id = this.selected()?.submissionId;
    if (!id || !newStatus) return;
    this.updatingStatus.set(true);
    this.svc.updateStatus(id, newStatus).subscribe({
      next: () => {
        // Update selected & list in-place
        this.selected.update(s => s ? { ...s, status: newStatus as SubmissionStatus } : s);
        this.submissions.update(list =>
          list.map(s => s.submissionId === id ? { ...s, status: newStatus as SubmissionStatus } : s)
        );
        // If new status is no longer in workbench range → remove from list & close
        if (!['IntakeComplete','UnderReview','Approved','Quoted','PolicyBound'].includes(newStatus)) {
          this.submissions.update(list => list.filter(s => s.submissionId !== id));
          this.selected.set(null);
        }
        this.updatingStatus.set(false);
        if (el) (el as HTMLSelectElement).value = '';
        this.flash('success', `Status changed to ${newStatus}`);
      },
      error: () => { this.updatingStatus.set(false); this.flash('danger', 'Status update failed.'); },
    });
  }

  closeDetail(): void { this.selected.set(null); }

  setDetailTab(t: DetailTab): void { this.detailTab.set(t); }

  // ── Subjectivity Create ────────────────────────────────────────────────────

  createSubjectivity(): void {
    const id = this.selected()?.submissionId;
    if (!id || !this.newSubjDesc().trim() || !this.newSubjDue()) return;
    this.creatingSubj.set(true);
    this.svc.createSubjectivity(id, this.newSubjDesc(), this.newSubjDue()).subscribe({
      next: res => {
        const s = res?.data;
        if (s) this.subjectivities.update(list => [s, ...list]);
        this.newSubjDesc.set(''); this.newSubjDue.set('');
        this.creatingSubj.set(false);
        this.flash('success', 'Subjectivity created!');
      },
      error: () => { this.creatingSubj.set(false); this.flash('danger', 'Create failed.'); },
    });
  }

  updateSubjStatus(sub: Subjectivity, status: string): void {
    this.svc.updateSubjectivityStatus(sub.subjectivityId, status).subscribe({
      next: () => {
        this.subjectivities.update(list =>
          list.map(s => s.subjectivityId === sub.subjectivityId ? { ...s, status: status as any } : s));
        this.flash('success', `Marked as ${status}`);
      },
      error: () => this.flash('danger', 'Update failed.'),
    });
  }

  // ── Note Create ────────────────────────────────────────────────────────────

  addNote(): void {
    const id = this.selected()?.submissionId;
    if (!id || !this.newNoteText().trim()) return;
    this.addingNote.set(true);
    this.svc.addNote(id, this.newNoteText()).subscribe({
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

  // ── Helpers ────────────────────────────────────────────────────────────────

  calculateRisk(): void {
    const id = this.selected()?.submissionId;
    if (!id) return;
    this.calculatingRisk.set(true);
    this.svc.calculateRiskScore(id).subscribe({
      next: res => { this.riskScore.set(res?.data ?? null); this.calculatingRisk.set(false); this.flash('success', 'Risk score calculated!'); },
      error: () => { this.calculatingRisk.set(false); this.flash('danger', 'Risk calculation failed.'); },
    });
  }

  evaluateRules(): void {
    const id = this.selected()?.submissionId;
    if (!id) return;
    this.evaluatingRules.set(true);
    this.svc.evaluateRules(id).subscribe({
      next: res => {
        const data = res?.data ?? res;
        this.rulesResult.set(data);
        this.evaluatingRules.set(false);
        this.flash('success', 'Rules evaluated successfully.');
      },
      error: () => { this.evaluatingRules.set(false); this.flash('danger', 'Rules evaluation failed.'); },
    });
  }

  submitDecision(): void {
    const id   = this.selected()?.submissionId;
    const dec  = this.decisionValue();
    const user = this.auth.currentUser();
    if (!id || !dec || !user) return;
    this.submittingDecision.set(true);
    this.svc.makeUWDecision({
      submissionId: id,
      decision:     dec,
      reason:       this.decisionReason(),
      decidedBy:    user.userId,
    }).subscribe({
      next: () => {
        this.decisionValue.set('');
        this.decisionReason.set('');

        // Map the UW decision to the appropriate submission status and update it.
        const newStatus = dec === 'Approve'  ? 'Approved'
                        : dec === 'Decline'  ? 'Declined'
                        : null; // Refer / MoreInfo — leave status unchanged

        if (newStatus) {
          this.svc.updateStatus(id, newStatus).subscribe({
            next: () => {
              // Update in-place so the badge refreshes immediately
              this.submissions.update(list =>
                list.map(s => s.submissionId === id
                  ? { ...s, status: newStatus as any }
                  : s));
              this.selected.update(s => s ? { ...s, status: newStatus as any } : s);

              // Declined submissions leave the workbench queue
              if (newStatus === 'Declined') {
                this.submissions.update(list => list.filter(s => s.submissionId !== id));
                this.selected.set(null);
              }

              this.submittingDecision.set(false);
              this.flash('success', `Decision recorded: ${dec} — status updated to ${newStatus}.`);
            },
            error: () => {
              this.submittingDecision.set(false);
              this.flash('success', `Decision recorded: ${dec}. (Status update failed — refresh manually.)`);
            },
          });
        } else {
          this.submittingDecision.set(false);
          this.flash('success', `Decision recorded: ${dec}`);
        }
      },
      error: () => { this.submittingDecision.set(false); this.flash('danger', 'Failed to record decision.'); },
    });
  }

  riskBandClass(band: string): string {
    return band === 'Low' ? 'bg-success' : band === 'Medium' ? 'bg-warning text-dark' : 'bg-danger';
  }

  riskBarWidth(score: number): number {
    return Math.min(100, Math.max(0, score));
  }

  riskBarColor(band: string): string {
    return band === 'Low' ? '#198754' : band === 'Medium' ? '#ffc107' : '#dc3545';
  }

  openSubjCount(id: string): number {
    return this.subjectivities().filter(s => s.status === 'Open').length;
  }

  subjStatusClass(s: string): string {
    return s === 'Met' ? 'bg-success' : s === 'Waived' ? 'bg-secondary' : 'bg-warning text-dark';
  }

  completenessClass(s: string): string {
    return s === 'Complete' ? 'text-success' : 'text-warning';
  }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }

  statusClass(s: string): string {
    const map: Record<string, string> = {
      Draft:         'bg-secondary',
      IntakeComplete:'bg-info text-dark',
      UnderReview:   'bg-warning text-dark',
      Approved:      'bg-success',
      Quoted:        'bg-primary',
      PolicyBound:   'bg-primary',
      Issued:        'bg-success',
      Declined:      'bg-danger',
      Expired:       'bg-dark',
    };
    return map[s] ?? 'bg-secondary';
  }

  // Data is loaded all-at-once; page/size changes just update the display counters.
  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }

  private flash(type: 'success'|'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 3500);
  }
}
