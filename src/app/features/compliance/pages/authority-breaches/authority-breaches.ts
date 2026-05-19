// Authority Breaches feature has been removed.
// This stub prevents build errors. The route has been removed from compliance.routes.ts.
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-authority-breaches',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container text-center py-5">
      <i class="bi bi-shield-slash fs-1 text-muted mb-3 d-block"></i>
      <h5 class="text-muted">This feature has been removed.</h5>
    </div>
  `,
})
export class AuthorityBreachesPage {}
