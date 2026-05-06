import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { UnderwritingApiService } from '../../services/underwriting-api.service';
import { UWDecision, UWNote, DecisionType } from '../../models/underwriting.model';

const DECISIONS: { value: DecisionType; label: string; css: string }[] = [
  { value: 'Approve',  label: 'Approve',      css: 'btn-success' },
  { value: 'Decline',  label: 'Decline',      css: 'btn-danger' },
  { value: 'Refer',    label: 'Refer to UW',  css: 'btn-warning' },
  { value: 'MoreInfo', label: 'Request Info', css: 'btn-outline-secondary' },
];

@Component({
  selector: 'app-uw-decision',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState],
  templateUrl: './uw-decision.html',
  styleUrl: './uw-decision.css',
})
export class UwDecisionPage implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly existingDecision = signal<UWDecision | null>(null);
  readonly notes            = signal<UWNote[]>([]);
  readonly loading          = signal(false);
  readonly saving           = signal(false);
  readonly submissionId     = signal('');
  readonly alertMsg         = signal<{ type: 'success'|'danger'; text: string } | null>(null);
  readonly decisions        = DECISIONS;

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
      next: res => { const d: any = res; this.existingDecision.set(d?.data ?? null); },
      error: () => {},
    });
  }

  private loadNotes(id: string): void {
    this.svc.getNotes(id).subscribe({
      next: res => { const d: any = res; this.notes.set(d?.data ?? []); },
      error: () => {},
    });
  }

  submitDecision(): void {
    if (this.decisionForm.invalid) { this.decisionForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.decisionForm.value;
    this.svc.submitDecision(this.submissionId(), {
      decision: v.decision as DecisionType, reason: v.reason!,
    }).subscribe({
      next: res => {
        const d: any = res;
        this.existingDecision.set(d?.data ?? null);
        this.saving.set(false);
        this.flash('success', 'Decision submitted.');
        this.router.navigate(['/underwriting/workbench']);
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Submit failed.'); },
    });
  }

  addNote(): void {
    if (this.noteForm.invalid) { this.noteForm.markAllAsTouched(); return; }
    this.svc.addNote(this.submissionId(), { noteText: this.noteForm.value.noteText! }).subscribe({
      next: () => { this.noteForm.reset(); this.loadNotes(this.submissionId()); },
      error: err => this.flash('danger', err?.error?.message ?? 'Note save failed.'),
    });
  }

  decisionClass(d: string): string {
    const map: Record<string, string> = {
      Approve: 'bg-success', Decline: 'bg-danger', Refer: 'bg-warning text-dark', MoreInfo: 'bg-secondary',
    };
    return map[d] ?? 'bg-secondary';
  }

  private flash(type: 'success'|'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
