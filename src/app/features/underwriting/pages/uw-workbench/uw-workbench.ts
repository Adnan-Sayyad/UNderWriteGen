import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import {
  Submission, Subjectivity, UWNote,
  Attachment, Questionnaire, CompletenessCheck, RiskScore
} from '../../../submission/models/submission.model';

type DetailTab = 'overview' | 'questionnaire' | 'attachments' | 'subjectivities' | 'notes';

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
    this.svc.getAll({ page, size: this.pageSize(), sort: 'createdDate', direction: 'desc' } as any).subscribe({
      next: res => {
        const all = res.content ?? [];
        const uw  = all.filter((s: any) =>
          ['IntakeComplete','UnderReview','Quoted'].includes(s.status));
        this.submissions.set(uw);
        this.totalPages.set(res.totalPages ?? 1);
        this.totalElements.set(uw.length);
        this.currentPage.set(page);
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
      Draft: 'bg-secondary', IntakeComplete: 'bg-info text-dark',
      UnderReview: 'bg-warning text-dark', Quoted: 'bg-primary',
      Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return map[s] ?? 'bg-secondary';
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

  private flash(type: 'success'|'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 3500);
  }
}
