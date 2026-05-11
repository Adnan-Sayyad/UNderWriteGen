import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { PricingApiService } from '../../services/pricing-api.service';
import { Quote, QuoteTerms, QuoteStatus } from '../../models/pricing.model';
import { AuthService } from '../../../../core/auth/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

/** Which inline panel is expanded */
type ActivePanel = 'terms' | 'status' | null;

const ALL_STATUSES: QuoteStatus[] = ['Draft', 'Presented', 'Accepted', 'Declined', 'Expired'];

@Component({
  selector: 'app-quote-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader],
  templateUrl: './quote-detail.html',
  styleUrl:    './quote-detail.css',
})
export class QuoteDetailPage implements OnInit {

  /* ── state ──────────────────────────────────────────────────────────── */
  readonly quoteId     = signal('');
  readonly quote       = signal<Quote | null>(null);
  readonly loading     = signal(true);
  readonly accepting   = signal(false);
  readonly saving      = signal(false);
  readonly activePanel = signal<ActivePanel>(null);
  readonly alertMsg    = signal<{ type: 'success' | 'danger' | 'warning'; text: string } | null>(null);

  readonly ALL_STATUSES = ALL_STATUSES;

  readonly breadcrumbs = [
    { label: 'Home',   route: '/' },
    { label: 'Quotes', route: '/pricing/quotes' },
    { label: 'Quote Detail' },
  ];

  /* ── computed — parsed JSON fields ─────────────────────────────────── */
  /**
   * Backend returns loadingsJson / discountsJson / taxesJson as JSON strings.
   * e.g. '{"riskLoading":500,"occupationLoading":200}'
   * We parse them and display as key-value rows.
   */
  readonly loadings = computed((): [string, number][] =>
    this.parseJsonPairs(this.quote()?.loadingsJson));

  readonly discounts = computed((): [string, number][] =>
    this.parseJsonPairs(this.quote()?.discountsJson));

  readonly taxes = computed((): [string, number][] =>
    this.parseJsonPairs(this.quote()?.taxesJson));

  readonly terms = computed((): QuoteTerms | null => {
    const raw = this.quote()?.termsJson;
    if (!raw) return null;
    try { return typeof raw === 'string' ? JSON.parse(raw) : (raw as QuoteTerms); }
    catch { return null; }
  });

  /** Gross premium before taxes */
  readonly grossPremium = computed(() => {
    const q = this.quote();
    if (!q) return 0;
    const loadSum = this.loadings().reduce((s, [, v]) => s + v, 0);
    const discSum = this.discounts().reduce((s, [, v]) => s + Math.abs(v), 0);
    return q.basePremium + loadSum - discSum;
  });

  readonly isEditable = computed(() => {
    const s = this.quote()?.status;
    return s === 'Draft' || s === 'Presented';
  });

  /* ── forms ──────────────────────────────────────────────────────────── */
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly termsForm = this.fb.group({
    deductibles:    [<number | null>null, [Validators.min(0)]],
    limitsJson:     ['{}'],
    exclusions:     [''],
    subjectivities: [''],
  });

  readonly statusForm = this.fb.group({
    status: ['', Validators.required],
    reason: ['', Validators.maxLength(250)],
  });

  constructor(private svc: PricingApiService, private route: ActivatedRoute) {}

  /* ── lifecycle ──────────────────────────────────────────────────────── */
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.quoteId.set(id);
    if (!id) { this.loading.set(false); return; }
    this.fetchQuote(id);
  }

  /* ── actions ────────────────────────────────────────────────────────── */
  accept(): void {
    this.accepting.set(true);
    this.svc.acceptQuote(this.quoteId()).subscribe({
      next: () => {
        this.accepting.set(false);
        this.quote.update(q => q ? { ...q, status: 'Accepted' } : q);
        this.flash('success', 'Quote accepted successfully.');
        this.activePanel.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.accepting.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to accept quote.');
      },
    });
  }

  saveTerms(): void {
    if (this.termsForm.invalid) { this.termsForm.markAllAsTouched(); return; }
    const v = this.termsForm.value;

    let limitsObj: Record<string, number> = {};
    try { limitsObj = JSON.parse(v.limitsJson || '{}'); } catch { /* ignore */ }

    const terms: QuoteTerms = {};
    if (v.deductibles != null && v.deductibles > 0) terms.deductibles = v.deductibles;
    if (Object.keys(limitsObj).length) terms.limits = limitsObj;
    if (v.exclusions?.trim())
      terms.exclusions = v.exclusions.split(',').map(s => s.trim()).filter(Boolean);
    if (v.subjectivities?.trim())
      terms.subjectivities = v.subjectivities.split('\n').map(s => s.trim()).filter(Boolean);

    this.saving.set(true);
    this.svc.updateTerms(this.quoteId(), {
      quoteId:  this.quoteId(),
      termsJson: JSON.stringify(terms),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.quote.update(q => q ? { ...q, termsJson: JSON.stringify(terms) } : q);
        this.flash('success', 'Quote terms saved.');
        this.activePanel.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to save terms.');
      },
    });
  }

  saveStatus(): void {
    if (this.statusForm.invalid) { this.statusForm.markAllAsTouched(); return; }
    const v = this.statusForm.value;
    this.saving.set(true);
    this.svc.updateStatus(this.quoteId(), {
      status: v.status!,
      reason: v.reason || undefined,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.quote.update(q => q ? { ...q, status: v.status as QuoteStatus } : q);
        this.flash('success', `Status updated to "${v.status}".`);
        this.activePanel.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.flash('danger', err.error?.error ?? 'Failed to update status.');
      },
    });
  }

  togglePanel(p: ActivePanel): void {
    const next = this.activePanel() === p ? null : p;
    if (next === 'terms')  this.prefillTermsForm();
    if (next === 'status') this.statusForm.patchValue({ status: this.quote()?.status ?? '' });
    this.activePanel.set(next);
  }

  /* ── helpers ────────────────────────────────────────────────────────── */
  statusClass(s: string): string {
    const m: Record<string, string> = {
      Draft: 'bg-secondary', Presented: 'bg-primary',
      Accepted: 'bg-success', Declined: 'bg-danger', Expired: 'bg-dark',
    };
    return m[s] ?? 'bg-secondary';
  }

  /** Convert camelCase key → readable label */
  fmtKey(k: string): string {
    return k.replace(/([A-Z])/g, ' $1').replace(/^./, c => c.toUpperCase());
  }

  canManage(): boolean {
    return this.auth.hasRole('Admin', 'Underwriter', 'Pricing');
  }

  isExpiredDate(): boolean {
    const q = this.quote();
    return !!q && new Date(q.validUntil) < new Date();
  }

  /* ── private ────────────────────────────────────────────────────────── */
  private fetchQuote(id: string): void {
    this.loading.set(true);
    this.svc.getQuote(id).subscribe({
      next: quote => {
        this.quote.set(quote);
        this.loading.set(false);
        this.syncToSession(quote);
      },
      error: () => {
        this.loading.set(false);
        this.flash('danger', 'Failed to load quote.');
      },
    });
  }

  private prefillTermsForm(): void {
    const t = this.terms();
    this.termsForm.patchValue({
      deductibles:    t?.deductibles ?? null,
      limitsJson:     t?.limits ? JSON.stringify(t.limits, null, 2) : '{}',
      exclusions:     t?.exclusions?.join(', ')  ?? '',
      subjectivities: t?.subjectivities?.join('\n') ?? '',
    });
  }

  private parseJsonPairs(raw?: string): [string, number][] {
    if (!raw) return [];
    try {
      const obj = typeof raw === 'string' ? JSON.parse(raw) : raw;
      return Object.entries(obj as Record<string, number>)
        .filter(([, v]) => v !== 0);
    } catch { return []; }
  }

  /** Keep session recent-quotes in sync so quote-list history is current */
  private syncToSession(q: Quote): void {
    try {
      const KEY = 'uwpro_recent_quotes';
      const entry = {
        quoteId: q.quoteId, submissionId: q.submissionId, versionNo: q.versionNo,
        basePremium: q.basePremium, totalPremium: q.totalPremium,
        validUntil: q.validUntil, status: q.status,
        searchedAt: new Date().toISOString(),
      };
      const raw  = sessionStorage.getItem(KEY);
      const list: any[] = raw ? JSON.parse(raw) : [];
      const deduped = list.filter(r => r.quoteId !== q.quoteId);
      sessionStorage.setItem(KEY, JSON.stringify([entry, ...deduped].slice(0, 20)));
    } catch { /* ignore */ }
  }

  private flash(type: 'success' | 'danger' | 'warning', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 5000);
  }
}
