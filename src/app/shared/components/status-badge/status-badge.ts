import { Component, Input } from '@angular/core';
import { StatusLabelPipe } from '../../pipes/status-label.pipe';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [StatusLabelPipe],
  template: `<span class="badge {{ status | statusLabel:'css' }}">{{ status | statusLabel }}</span>`,
})
export class StatusBadge {
  @Input({ required: true }) status!: string;
}
