import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="text-center py-5 text-muted">
      <i class="bi {{ icon }} fs-1 d-block mb-3 opacity-25"></i>
      <h6>{{ title }}</h6>
      @if (message) { <p class="small mb-0">{{ message }}</p> }
    </div>
  `,
})
export class EmptyState {
  @Input() icon = 'bi-inbox';
  @Input() title = 'No records found';
  @Input() message?: string;
}
