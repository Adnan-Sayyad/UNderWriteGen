import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { NotificationApiService } from '../../services/notification-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { Notification, NotificationCategory, NotificationStatus } from '../../models/notification.model';

const CATEGORIES: NotificationCategory[] = ['Referral', 'SLA', 'Subjectivity', 'Quote', 'Compliance'];

// Roles that may create/send notifications.
const CREATOR_ROLES = ['Admin', 'Underwriter', 'UWManager', 'UWAssistant', 'Compliance', 'Operations'];

type ViewFilter = NotificationStatus | '' | 'sent' | 'received';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, EmptyState, Pagination],
  templateUrl: './notification-center.html',
  styleUrl: './notification-center.css',
})
export class NotificationCenterPage implements OnInit {
  readonly notifications = signal<Notification[]>([]);
  readonly loading       = signal(false);
  readonly saving        = signal(false);
  readonly filterStatus  = signal<ViewFilter>('');
  readonly showCreate    = signal(false);
  readonly alertMsg      = signal<{ type: 'success' | 'danger'; text: string } | null>(null);
  readonly searchQuery   = signal('');
  readonly sendMode      = signal<'individual' | 'group'>('individual');
  readonly currentPage   = signal(0);
  readonly pageSize      = signal(10);

  readonly categories = CATEGORIES;
  readonly roleGroups = [
    { label: 'All Agents',         value: 'Agent' },
    { label: 'All Underwriters',   value: 'Underwriter' },
    { label: 'All UW Managers',    value: 'UWManager' },
    { label: 'All UW Assistants',  value: 'UWAssistant' },
    { label: 'All Compliance',     value: 'Compliance' },
    { label: 'All Operations',     value: 'Operations' },
    { label: 'All Admins',         value: 'Admin' },
    { label: 'Everyone',           value: 'Everyone' },
  ];

  private readonly authSvc = inject(AuthService);
  private readonly fb      = inject(FormBuilder);
  private readonly svc     = inject(NotificationApiService);

  // Current user's email — used to distinguish sent vs received.
  readonly currentUserEmail = computed(() => this.authSvc.currentUser()?.email ?? '');

  // Only show the "Send Notification" button for authorised creator roles.
  readonly canCreate = computed(() => {
    const user = this.authSvc.currentUser();
    return !!user && CREATOR_ROLES.includes(user.role);
  });

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    const q = this.searchQuery().trim().toLowerCase();
    const me = this.currentUserEmail();

    return this.notifications().filter(n => {
      const matchesStatus =
        s === ''          ? true :
        s === 'sent'      ? n.senderEmail === me :
        s === 'received'  ? n.recipientEmail === me :
        n.status === s;

      const matchesQuery = !q || [n.message, n.recipientEmail, n.senderEmail]
        .some(v => v.toLowerCase().includes(q));

      return matchesStatus && matchesQuery;
    });
  });

  // Unread count counts only RECEIVED notifications (drives the bell badge).
  readonly unreadCount = computed(() =>
    this.notifications().filter(
      n => n.status === 'Unread' && n.recipientEmail === this.currentUserEmail()
    ).length
  );

  readonly sentCount = computed(() =>
    this.notifications().filter(n => n.senderEmail === this.currentUserEmail()).length
  );

  readonly totalElements = computed(() => this.filtered().length);
  readonly totalPages    = computed(() => Math.max(1, Math.ceil(this.totalElements() / this.pageSize())));
  readonly paginated     = computed(() => {
    const size = this.pageSize();
    const page = Math.min(this.currentPage(), this.totalPages() - 1);
    return this.filtered().slice(page * size, page * size + size);
  });

  readonly filterOptions: Array<{ label: string; value: ViewFilter }> = [
    { label: 'All',       value: '' },
    { label: 'Received',  value: 'received' },
    { label: 'Sent',      value: 'sent' },
    { label: 'Unread',    value: 'Unread' },
    { label: 'Read',      value: 'Read' },
    { label: 'Dismissed', value: 'Dismissed' },
  ];

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Notifications' },
  ];

  // fb must be declared before form to avoid init-order errors
  readonly form = this.fb.group({
    recipientEmail: ['', [Validators.required, Validators.email]],
    recipientGroup: ['Agent'],
    message:        ['', Validators.required],
    category:       ['Referral' as NotificationCategory, Validators.required],
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getMy().subscribe({
      next: res => {
        const d: any = res;
        this.notifications.set(d?.data ?? d?.content ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // True when the logged-in user is the sender of this notification.
  isSent(n: Notification): boolean {
    return n.senderEmail === this.currentUserEmail();
  }

  // True when the logged-in user is the recipient of this notification.
  isReceived(n: Notification): boolean {
    return n.recipientEmail === this.currentUserEmail();
  }

  markRead(n: Notification): void {
    if (n.status !== 'Unread') return;
    this.svc.markRead(n.notificationID).subscribe({ next: () => this.load() });
  }

  dismiss(n: Notification): void {
    if (n.status === 'Dismissed') return;
    this.svc.dismiss(n.notificationID).subscribe({ next: () => this.load() });
  }

  deleteNotification(n: Notification): void {
    this.svc.delete(n.notificationID).subscribe({
      next: () => { this.load(); this.flash('success', 'Notification deleted.'); },
    });
  }

  openCreate(): void {
    this.sendMode.set('individual');
    this.form.reset({ category: 'Referral', recipientGroup: 'Agent' });
    this.showCreate.set(true);
  }

  setSendMode(mode: 'individual' | 'group'): void {
    this.sendMode.set(mode);
  }

  closeCreate(): void { this.showCreate.set(false); }

  saveNotification(): void {
    const v = this.form.value;
    const mode = this.sendMode();

    // Validate: in individual mode, email is required; in group mode, only message+category
    if (mode === 'individual') {
      if (!v.recipientEmail || !v.message || !v.category) {
        this.form.markAllAsTouched();
        return;
      }
    } else {
      if (!v.message || !v.category) {
        this.form.markAllAsTouched();
        return;
      }
    }

    this.saving.set(true);

    const onSuccess = (res: any) => {
      this.saving.set(false);
      this.closeCreate();
      this.load();
      const msg = mode === 'group'
        ? `Notification broadcast to ${res?.data?.sentCount ?? 'all'} user(s).`
        : 'Notification sent successfully.';
      this.flash('success', msg);
    };

    const onError = (err: any) => {
      this.saving.set(false);
      this.flash('danger', err?.error?.message ?? 'Failed to send notification.');
    };

    if (mode === 'individual') {
      this.svc.create({
        recipientEmail: v.recipientEmail!,
        message:        v.message!,
        category:       v.category!,
      }).subscribe({ next: onSuccess, error: onError });
    } else {
      this.svc.broadcast({
        recipientGroup: v.recipientGroup!,
        message:        v.message!,
        category:       v.category!,
      }).subscribe({ next: onSuccess, error: onError });
    }
  }

  onPageChange(p: number): void { this.currentPage.set(p); }
  onSizeChange(s: number): void { this.pageSize.set(s); this.currentPage.set(0); }

  setFilter(v: string): void { this.filterStatus.set(v as ViewFilter); this.currentPage.set(0); }

  onSearch(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
    this.currentPage.set(0);
  }

  clearSearch(): void { this.searchQuery.set(''); this.currentPage.set(0); }

  categoryIcon(cat: string): string {
    const map: Record<string, string> = {
      Referral:    'bi-send',
      SLA:         'bi-alarm',
      Subjectivity:'bi-list-check',
      Quote:       'bi-calculator',
      Compliance:  'bi-clipboard2-check',
    };
    return map[cat] ?? 'bi-bell';
  }

  categoryClass(cat: string): string {
    const map: Record<string, string> = {
      Referral:    'bg-warning text-dark',
      SLA:         'bg-danger',
      Subjectivity:'bg-info text-dark',
      Quote:       'bg-primary',
      Compliance:  'bg-success',
    };
    return map[cat] ?? 'bg-secondary';
  }

  private flash(type: 'success' | 'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
