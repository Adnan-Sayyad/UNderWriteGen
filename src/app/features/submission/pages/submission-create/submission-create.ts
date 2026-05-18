import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { SubmissionApiService } from '../../services/submission-api.service';
import { ProductLine, DocType, Attachment, CompletenessCheck } from '../../models/submission.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { PartyApiService } from '../../../party/services/party-api.service';
import { CustomerParty, Agent } from '../../../party/models/party.model';

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
export class SubmissionCreatePage implements OnInit {
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);

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

  readonly isAgent = signal(false);

  // ── Party Search ───────────────────────────────────────────────────────────
  readonly partySearchText  = signal('');
  readonly partyResults     = signal<CustomerParty[]>([]);
  readonly searchingParties = signal(false);
  readonly selectedParty    = signal<CustomerParty | null>(null);
  readonly showPartyDrop    = signal(false);
  private partyTimer: any;

  // ── Agent Search ───────────────────────────────────────────────────────────
  readonly agentSearchText  = signal('');
  readonly agentResults     = signal<Agent[]>([]);
  readonly searchingAgents  = signal(false);
  readonly selectedAgent    = signal<Agent | null>(null);
  readonly showAgentDrop    = signal(false);
  private agentTimer: any;

  constructor(
    private svc: SubmissionApiService,
    private router: Router,
    private partySvc: PartyApiService,
  ) {}

  ngOnInit(): void {
    const user = this.auth.currentUser();
    if (user?.role === 'Agent') {
      this.isAgent.set(true);
      // Auto-select the agent card using cached agentID (set during login)
      const cacheKey = `uwpro_agent_id_${user.userId}`;
      const agentId  = localStorage.getItem(cacheKey)
                    ?? localStorage.getItem('uwpro_my_agent_id');
      if (agentId) {
        // Fetch the agent record and pre-select it
        this.partySvc.getAgent(agentId).subscribe({
          next: (res: any) => {
            const agent = res?.data ?? res;
            if (agent?.agentID || agent?.agentId) {
              this.selectAgent({
                agentID:      agent.agentID ?? agent.agentId,
                name:         agent.name    ?? agent.Name    ?? '',
                producerCode: agent.producerCode ?? agent.ProducerCode ?? '',
                region:       agent.region       ?? agent.Region       ?? '',
                status:       agent.status       ?? agent.Status       ?? 'Active',
                contactInfo:  agent.contactInfo  ?? '',
              });
            } else {
              this.autoSearchAgentByName(user);
            }
          },
          error: () => this.autoSearchAgentByName(user),
        });
      } else {
        // No cache yet — auto-search by the logged-in user's name
        this.autoSearchAgentByName(user);
      }
    }
  }

  /** Auto-search by the agent's display name and pre-select if exactly one match. */
  private autoSearchAgentByName(user: any): void {
    const name = user?.name ?? user?.email?.split('@')[0] ?? '';
    if (!name) return;
    this.agentSearchText.set(name);
    this.searchingAgents.set(true);
    this.partySvc.getAgents(name).subscribe({
      next: (res: any) => {
        const items: Agent[] = Array.isArray(res) ? res
          : Array.isArray(res?.data) ? res.data
          : [];
        this.searchingAgents.set(false);
        if (items.length === 1) {
          // Only one match — auto-select silently (no dropdown shown)
          this.selectAgent(items[0]);
        } else if (items.length > 1) {
          // Multiple matches — show dropdown so agent can pick
          this.agentResults.set(items);
          this.showAgentDrop.set(true);
        }
        // Zero matches — leave search text so agent sees what was searched
      },
      error: () => { this.searchingAgents.set(false); },
    });
  }

  onPartySearch(text: string): void {
    this.partySearchText.set(text);
    this.selectedParty.set(null);
    this.form.patchValue({ partyId: '' });
    clearTimeout(this.partyTimer);
    if (!text.trim()) { this.partyResults.set([]); this.showPartyDrop.set(false); return; }
    this.partyTimer = setTimeout(() => {
      this.searchingParties.set(true);
      this.showPartyDrop.set(true);
      this.partySvc.getParties(text).subscribe({
        next: (res: any) => {
          const items: CustomerParty[] = Array.isArray(res) ? res : (res?.data ?? []);
          this.partyResults.set(items);
          this.searchingParties.set(false);
        },
        error: () => { this.searchingParties.set(false); this.partyResults.set([]); },
      });
    }, 350);
  }

  selectParty(p: CustomerParty): void {
    this.selectedParty.set(p);
    this.partySearchText.set(p.name);
    this.form.patchValue({ partyId: p.partyID });
    this.showPartyDrop.set(false);
    this.partyResults.set([]);
  }

  clearParty(): void {
    this.selectedParty.set(null);
    this.partySearchText.set('');
    this.form.patchValue({ partyId: '' });
    this.partyResults.set([]);
    this.showPartyDrop.set(false);
  }

  onAgentSearch(text: string): void {
    this.agentSearchText.set(text);
    this.selectedAgent.set(null);
    this.form.patchValue({ agentId: '' });
    clearTimeout(this.agentTimer);
    if (!text.trim()) { this.agentResults.set([]); this.showAgentDrop.set(false); return; }
    this.agentTimer = setTimeout(() => {
      this.searchingAgents.set(true);
      this.showAgentDrop.set(true);
      this.partySvc.getAgents(text).subscribe({
        next: (res: any) => {
          const items: Agent[] = Array.isArray(res) ? res : (res?.data ?? []);
          this.agentResults.set(items);
          this.searchingAgents.set(false);
        },
        error: () => { this.searchingAgents.set(false); this.agentResults.set([]); },
      });
    }, 350);
  }

  selectAgent(a: Agent): void {
    this.selectedAgent.set(a);
    this.agentSearchText.set(a.name);
    this.form.patchValue({ agentId: a.agentID });
    this.showAgentDrop.set(false);
    this.agentResults.set([]);
    // Save agentID to localStorage so submission-list can filter correctly
    if (this.isAgent()) {
      const userId = this.auth.currentUser()?.userId ?? '';
      localStorage.setItem(`uwpro_agent_id_${userId}`, a.agentID);
      localStorage.setItem('uwpro_my_agent_id', a.agentID); // backward compat
    }
  }

  clearAgent(): void {
    this.selectedAgent.set(null);
    this.agentSearchText.set('');
    this.form.patchValue({ agentId: '' });
    this.agentResults.set([]);
    this.showAgentDrop.set(false);
  }

  // ── Step 1: Create Submission ──────────────────────────────────────────────

  createSubmission(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.form.getRawValue(); // getRawValue includes disabled fields
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
      error: err => {
        this.saving.set(false);
        console.error('❌ Submission create failed — full error:', err);
        console.error('❌ Status:', err?.status);
        console.error('❌ Error body:', err?.error);
        console.error('❌ Message:', err?.error?.message ?? err?.message);
        this.flash('danger', err?.error?.message ?? err?.error?.Message ?? `Create failed (${err?.status})`);
      },
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
    this.advanceToIntakeComplete(() =>
      this.router.navigate(['/submissions', this.submissionId])
    );
  }

  finishToList(): void {
    this.advanceToIntakeComplete(() =>
      this.router.navigate(['/submissions'])
    );
  }

  /** Advance status to IntakeComplete so the submission enters the UW Workbench queue,
   *  then run the callback regardless of success/failure so the user isn't blocked. */
  private advanceToIntakeComplete(then: () => void): void {
    if (!this.submissionId) { then(); return; }
    this.svc.updateStatus(this.submissionId, 'IntakeComplete').subscribe({
      next:  () => then(),
      error: () => then(),   // navigate anyway; status can be fixed manually
    });
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
