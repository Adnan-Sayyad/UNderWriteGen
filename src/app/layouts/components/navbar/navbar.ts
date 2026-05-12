import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationApiService } from '../../../features/notifications/services/notification-api.service';

const POLL_INTERVAL_MS = 30_000;

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
})
export class Navbar implements OnInit, OnDestroy {
  readonly user = computed(() => this.auth.currentUser());
  readonly unreadCount = signal(0);

  private readonly notifications = inject(NotificationApiService);
  private sub?: Subscription;

  constructor(readonly auth: AuthService) {}

  ngOnInit(): void {
    // Poll the unread badge every 30s so users see fresh events without a manual refresh.
    if (!this.auth.isAuthenticated()) return;
    this.sub = interval(POLL_INTERVAL_MS).pipe(
      startWith(0),
      switchMap(() => this.notifications.getUnreadCount()),
    ).subscribe({
      next: res => this.unreadCount.set(typeof res === 'number' ? res : (res?.data ?? 0)),
      error: () => { /* non-blocking: keep the last value */ },
    });
  }

  ngOnDestroy(): void { this.sub?.unsubscribe(); }
}
