import { Component, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="page-container d-flex flex-column align-items-center justify-content-center"
         style="min-height:60vh; text-align:center;">
      <div class="mb-4" style="font-size:4rem; color:var(--uwpro-primary); opacity:.25;">
        <i class="bi bi-shield-lock-fill"></i>
      </div>
      <h2 class="fw-bold mb-2">Access Denied</h2>
      <p class="text-muted mb-1">You do not have permission to view this page.</p>
      <p class="text-muted small mb-4">
        Logged in as <strong>{{ user()?.name }}</strong>
        &mdash; Role: <span class="badge" style="background:var(--uwpro-primary)">{{ user()?.role }}</span>
      </p>
      <a routerLink="/submissions" class="btn btn-primary px-4">
        <i class="bi bi-arrow-left me-2"></i>Back to Dashboard
      </a>
    </div>
  `,
})
export class AccessDeniedPage {
  readonly user = computed(() => this.auth.currentUser());
  constructor(readonly auth: AuthService) {}
}
