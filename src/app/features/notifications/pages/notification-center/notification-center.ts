import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { NotificationApiService } from '../../services/notification-api.service';
import { Notification, NotificationCategory, NotificationStatus } from '../../models/notification.model';
import { DEFAULT_PAGE_REQUEST } from '../../../../shared/models/pagination.model';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, RouterModule, PageHeader, EmptyState],
  templateUrl: './notification-center.html',
  styleUrl: './notification-center.css',
})
export class NotificationCenterPage implements OnInit {
  readonly notifications = signal<Notification[]>([]);
  readonly loading       = signal(false);
  readonly filterStatus  = signal<NotificationStatus | ''>('');

  readonly filtered = computed(() => {
    const s = this.filterStatus();
    return this.notifications().filter(n => !s || n.status === s);
  });

  readonly unreadCount = computed(() => this.notifications().filter(n => n.status === 'Unread').length);

  readonly filterOptions: Array<NotificationStatus | ''> = ['', 'Unread', 'Read', 'Dismissed'];

  readonly breadcrumbs = [
    { label: 'Home', route: '/' },
    { label: 'Notifications' },
  ];

  constructor(private svc: NotificationApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getAll(DEFAULT_PAGE_REQUEST).subscribe({
      next: res => {
        const d: any = res;
        this.notifications.set(d?.content ?? d?.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  markRead(n: Notification): void {
    if (n.status !== 'Unread') return;
    this.svc.markRead(n.notificationId).subscribe({ next: () => this.load() });
  }

  markAllRead(): void {
    this.svc.markAllRead().subscribe({ next: () => this.load() });
  }

  dismiss(n: Notification): void {
    this.svc.dismiss(n.notificationId).subscribe({ next: () => this.load() });
  }

  setFilter(s: string): void { this.filterStatus.set(s as NotificationStatus | ''); }

  categoryIcon(cat: string): string {
    const map: Record<string, string> = {
      Referral: 'bi-send', SLA: 'bi-alarm', Subjectivity: 'bi-list-check',
      Quote: 'bi-calculator', Compliance: 'bi-clipboard2-check',
    };
    return map[cat] ?? 'bi-bell';
  }

  categoryClass(cat: string): string {
    const map: Record<string, string> = {
      Referral: 'bg-warning text-dark', SLA: 'bg-danger', Subjectivity: 'bg-info text-dark',
      Quote: 'bg-primary', Compliance: 'bg-success',
    };
    return map[cat] ?? 'bg-secondary';
  }
}
