import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { switchMap, of } from 'rxjs';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { PolicyApiService } from '../../services/policy-api.service';
import { Policy, PolicyStatus } from '../../models/policy.model';
import { PricingApiService } from '../../../pricing/services/pricing-api.service';
import { Quote } from '../../../pricing/models/pricing.model';
import { SubmissionApiService } from '../../../submission/services/submission-api.service';
import { PartyApiService } from '../../../party/services/party-api.service';

const STATUSES: PolicyStatus[] = ['Active', 'Cancelled', 'Expired'];
const PRODUCT_LINES = ['Life', 'Health', 'PnC', 'Commercial'];

@Component({
  selector: 'app-policy-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './policy-list.html',
  styleUrl: './policy-list.css',
})
export class PolicyListPage implements OnInit {
  readonly policies      = signal<Policy[]>([]);
  readonly loading       = signal(false);
  readonly filterStatus  = signal('');
  readonly searchQuery   = signal('');
  readonly statuses      = STATUSES;
  readonly productLines  = PRODUCT_LINES;
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);
  readonly totalPages    = signal(0);
  readonly totalElements = signal(0);
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly showBindModal = signal(false);
  readonly binding       = signal(false);

  // Bind form fields
  readonly bindSubId         = signal('');
  readonly bindPolicyNumber  = signal('');
  readonly bindProductLine   = signal('Life');
  readonly bindInceptionDate = signal('');
  readonly bindExpiryDate    = signal('');

  // Accepted quotes for bind dropdown
  readonly acceptedQuotes = signal<Quote[]>([]);
  readonly loadingQuoted  = signal(false);
  readonly bindQuoteId    = signal('');

  // Post-bind document state
  readonly boundPolicy    = signal<Policy | null>(null);
  readonly boundQuote     = signal<Quote | null>(null);
  readonly showDocModal   = signal(false);

  readonly filtered = computed(() => {
    const q = this.searchQuery().toLowerCase();
    const s = this.filterStatus();
    return this.policies().filter(p =>
      (!q || p.policyNumber?.toLowerCase().includes(q)) && (!s || p.status === s)
    );
  });

  readonly pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i)
  );

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Policy Desk' },
  ];

  constructor(
    private svc: PolicyApiService,
    private pricingSvc: PricingApiService,
    private submissionSvc: SubmissionApiService,
    private partySvc: PartyApiService,
  ) {}

  ngOnInit(): void { this.load(); }

  load(page = this.currentPage()): void {
    this.loading.set(true);
    const req: any = { page, size: this.pageSize(), sort: 'createdDate', direction: 'desc' };
    this.svc.getAll(req).subscribe({
      next: (res: any) => {
        this.policies.set(res?.content ?? res?.data ?? []);
        this.totalPages.set(res?.totalPages ?? 1);
        this.totalElements.set(res?.totalElements ?? 0);
        this.currentPage.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onPageChange(p: number): void { this.currentPage.set(p); this.load(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); this.load(0); }

  openBindModal(): void {
    this.showBindModal.set(true);
    this.bindQuoteId.set('');
    this.bindSubId.set('');
    this.bindPolicyNumber.set('');
    this.bindProductLine.set('Life');
    this.bindInceptionDate.set('');
    this.bindExpiryDate.set('');
    this.loadAcceptedQuotes();
  }

  closeBindModal(): void { this.showBindModal.set(false); }

  loadAcceptedQuotes(): void {
    this.loadingQuoted.set(true);
    this.pricingSvc.getAllQuotes('Accepted').subscribe({
      next: (res: any) => {
        const items: Quote[] = Array.isArray(res) ? res : (res?.data ?? res?.content ?? []);
        this.acceptedQuotes.set(items.filter((q: any) => q.status === 'Accepted'));
        this.loadingQuoted.set(false);
      },
      error: () => this.loadingQuoted.set(false),
    });
  }

  onQuoteSelect(quoteId: string): void {
    this.bindQuoteId.set(quoteId);
    const q = this.acceptedQuotes().find(x => x.quoteId === quoteId);
    if (q) {
      this.bindSubId.set(q.submissionId);
    }
  }

  bindPolicy(): void {
    const sid = this.bindSubId().trim();
    const pn  = this.bindPolicyNumber().trim();
    const pl  = this.bindProductLine();
    const id  = this.bindInceptionDate();
    const ed  = this.bindExpiryDate();
    if (!this.bindQuoteId() || !sid || !pn || !pl || !id || !ed) {
      this.flash('danger', 'All fields are required to bind a policy.');
      return;
    }
    this.binding.set(true);
    const selectedQuote = this.acceptedQuotes().find(q => q.quoteId === this.bindQuoteId()) ?? null;
    this.svc.bind(sid, pn, pl, id, ed).subscribe({
      next: (res: any) => {
        this.binding.set(false);
        const policy: Policy = res?.data ?? res;
        this.boundPolicy.set(policy);
        this.boundQuote.set(selectedQuote);
        this.closeBindModal();
        this.showDocModal.set(true);
        this.load(0);
      },
      error: (err: any) => {
        this.binding.set(false);
        this.flash('danger', err?.error?.message ?? 'Failed to bind policy.');
      },
    });
  }

  closeDocModal(): void { this.showDocModal.set(false); }

  /** Called from the success modal after a fresh bind. */
  downloadPolicyDoc(): void {
    const p = this.boundPolicy();
    if (!p) return;
    this.fetchPartyNameAndOpen(p, this.boundQuote());
  }

  /** Called from each table row — no quote data available for old policies. */
  downloadDoc(p: Policy): void {
    this.fetchPartyNameAndOpen(p, null);
  }

  /** Resolves party name via submission → party, then opens the cover note. */
  private fetchPartyNameAndOpen(p: Policy, q: Quote | null): void {
    this.submissionSvc.getById(p.submissionId).pipe(
      switchMap((subRes: any) => {
        const partyId: string = subRes?.data?.partyId ?? subRes?.partyId ?? subRes?.data?.partyID ?? subRes?.partyID ?? '';
        if (!partyId) return of('—');
        return this.partySvc.getParty(partyId).pipe(
          switchMap((partyRes: any) => {
            const name: string = partyRes?.data?.name ?? partyRes?.name ?? '—';
            return of(name);
          })
        );
      })
    ).subscribe({
      next:  (partyName: string) => this.openCoverNote(p, q, partyName),
      error: ()                  => this.openCoverNote(p, q, '—'),
    });
  }

  private openCoverNote(p: Policy, q: Quote | null, partyName = '—'): void {
    const fmt = (d: string) => d ? new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'long', year: 'numeric' }) : '—';
    const currency = (n: number) => '₹' + (n ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2 });
    const issueDate = new Date().toLocaleDateString('en-IN', { day: '2-digit', month: 'long', year: 'numeric' });

    const html = `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8"/>
<title>Policy Cover Note – ${p.policyNumber}</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
  *{box-sizing:border-box;margin:0;padding:0}
  body{font-family:'Inter',sans-serif;background:#f4f6fa;color:#1a2233;padding:32px}
  .doc{max-width:720px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.10)}
  .header{background:linear-gradient(135deg,#1a6e3c 0%,#28a745 100%);padding:32px 40px;color:#fff}
  .header-top{display:flex;justify-content:space-between;align-items:flex-start}
  .brand{display:flex;align-items:center;gap:10px}
  .brand-icon{width:40px;height:40px;background:rgba(255,255,255,.20);border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:20px}
  .brand-name{font-size:20px;font-weight:700;letter-spacing:.4px}
  .doc-type{text-align:right}
  .doc-type h1{font-size:22px;font-weight:700}
  .doc-type p{opacity:.85;font-size:13px;margin-top:2px}
  .divider{border:none;border-top:1px solid rgba(255,255,255,.25);margin:20px 0}
  .header-meta{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}
  .meta-item label{font-size:11px;opacity:.75;text-transform:uppercase;letter-spacing:.8px}
  .meta-item p{font-size:14px;font-weight:600;margin-top:3px}
  .body{padding:32px 40px}
  .section-title{font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:.8px;color:#6c757d;margin-bottom:12px;padding-bottom:6px;border-bottom:1px solid #e9ecef}
  .grid2{display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:24px}
  .grid3{display:grid;grid-template-columns:1fr 1fr 1fr;gap:8px;margin-bottom:24px}
  .field{background:#f8f9fa;border-radius:8px;padding:12px 14px}
  .field label{font-size:11px;color:#6c757d;text-transform:uppercase;letter-spacing:.6px}
  .field p{font-size:14px;font-weight:600;color:#1a2233;margin-top:3px}
  .badge{display:inline-block;padding:3px 10px;border-radius:20px;font-size:12px;font-weight:600}
  .badge-success{background:#d1fae5;color:#065f46}
  .badge-info{background:#dbeafe;color:#1e40af}
  .premium-box{background:linear-gradient(135deg,#f0fdf4,#dcfce7);border:1px solid #bbf7d0;border-radius:10px;padding:18px 20px;display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}
  .premium-label{font-size:13px;color:#166534;font-weight:600}
  .premium-value{font-size:26px;font-weight:700;color:#166534}
  .notice{background:#fffbeb;border:1px solid #fde68a;border-radius:8px;padding:14px 16px;font-size:12px;color:#92400e;line-height:1.6;margin-bottom:24px}
  .notice strong{font-weight:700}
  .footer{background:#f8f9fa;padding:16px 40px;display:flex;justify-content:space-between;align-items:center;border-top:1px solid #e9ecef}
  .footer p{font-size:11px;color:#9ca3af}
  .watermark{font-size:11px;font-weight:600;color:#9ca3af;text-transform:uppercase;letter-spacing:1px}
  @media print{body{background:#fff;padding:0}.doc{box-shadow:none;border-radius:0}}
</style>
</head>
<body>
<div class="doc">
  <!-- HEADER -->
  <div class="header">
    <div class="header-top">
      <div class="brand">
        <div class="brand-icon">🛡</div>
        <span class="brand-name">UnderwritePro</span>
      </div>
      <div class="doc-type">
        <h1>Policy Cover Note</h1>
        <p>Issue Date: ${issueDate}</p>
      </div>
    </div>
    <hr class="divider"/>
    <div class="header-meta">
      <div class="meta-item">
        <label>Policy Number</label>
        <p>${p.policyNumber ?? '—'}</p>
      </div>
      <div class="meta-item">
        <label>Product Line</label>
        <p>${p.productLine ?? '—'}</p>
      </div>
      <div class="meta-item">
        <label>Status</label>
        <p>${p.status ?? 'Active'}</p>
      </div>
    </div>
  </div>

  <!-- BODY -->
  <div class="body">

    <!-- Policy Period -->
    <div class="section-title">Policy Period</div>
    <div class="grid2">
      <div class="field"><label>Inception Date</label><p>${fmt(p.inceptionDate)}</p></div>
      <div class="field"><label>Expiry Date</label><p>${fmt(p.expiryDate)}</p></div>
    </div>

    <!-- Policyholder -->
    <div class="section-title">Policyholder</div>
    <div class="grid2" style="margin-bottom:24px">
      <div class="field"><label>Party / Client Name</label><p>${partyName}</p></div>
      <div class="field"><label>Submission Ref</label><p style="font-size:11px;word-break:break-all">${(p.submissionId ?? '').slice(0,18)}…</p></div>
    </div>

    <!-- Reference Numbers -->
    <div class="section-title">Reference Numbers</div>
    <div class="grid2">
      <div class="field"><label>Policy ID</label><p style="font-size:11px;word-break:break-all">${p.policyId ?? '—'}</p></div>
      <div class="field"><label>Quote Ref</label><p>${q?.quoteRef ?? (q?.quoteId ? (q.quoteId).slice(0,14) : '—')}</p></div>
    </div>

    ${q ? `
    <!-- Premium -->
    <div class="section-title">Premium Summary</div>
    <div class="premium-box">
      <div>
        <div class="premium-label">Total Annual Premium</div>
        <div style="font-size:12px;color:#166534;margin-top:2px">Quote valid till ${fmt(q.validUntil)}</div>
      </div>
      <div class="premium-value">${currency(q.totalPremium)}</div>
    </div>` : ''}

    <!-- Important Notice -->
    <div class="notice">
      <strong>Important Notice:</strong> This Cover Note is a summary document issued for reference purposes only.
      It confirms that a policy has been bound in the UnderwritePro system. This document does not constitute
      a full policy contract. Full policy terms, conditions and exclusions are governed by the policy schedule
      and wording issued separately. For queries, contact your assigned agent or the Operations team.
    </div>

    <!-- Validity -->
    <div class="section-title">Document Validity</div>
    <div class="grid2">
      <div class="field"><label>Issued On</label><p>${issueDate}</p></div>
      <div class="field"><label>Issued By</label><p>UnderwritePro Operations</p></div>
    </div>

  </div>

  <!-- FOOTER -->
  <div class="footer">
    <p>This is a system-generated document. No signature required.</p>
    <span class="watermark">UnderwritePro &copy; ${new Date().getFullYear()}</span>
  </div>
</div>
<script>window.onload=()=>{window.print();}</script>
</body>
</html>`;

    const blob = new Blob([html], { type: 'text/html' });
    const url  = URL.createObjectURL(blob);
    const win  = window.open(url, '_blank');
    if (win) { win.focus(); }
    setTimeout(() => URL.revokeObjectURL(url), 60000);
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'bg-success' : s === 'Cancelled' ? 'bg-danger' : 'bg-secondary';
  }

  productClass(p: string): string {
    const map: Record<string, string> = {
      Life: 'bg-success', Health: 'bg-info text-dark',
      PnC: 'bg-warning text-dark', Commercial: 'bg-primary',
    };
    return map[p] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string) {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
