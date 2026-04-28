import { Pipe, PipeTransform } from '@angular/core';

const STATUS_MAP: Record<string, { label: string; css: string }> = {
  Draft:          { label: 'Draft',           css: 'badge-draft' },
  IntakeComplete: { label: 'Intake Complete', css: 'badge-approved' },
  UnderReview:    { label: 'Under Review',    css: 'badge-pending' },
  Quoted:         { label: 'Quoted',          css: 'badge-approved' },
  Declined:       { label: 'Declined',        css: 'badge-declined' },
  Expired:        { label: 'Expired',         css: 'badge-expired' },
  Active:         { label: 'Active',          css: 'badge-active' },
  Cancelled:      { label: 'Cancelled',       css: 'badge-declined' },
  Pending:        { label: 'Pending',         css: 'badge-pending' },
  Approved:       { label: 'Approved',        css: 'badge-approved' },
  Rejected:       { label: 'Rejected',        css: 'badge-declined' },
  Offered:        { label: 'Offered',         css: 'badge-approved' },
  Accepted:       { label: 'Accepted',        css: 'badge-active' },
  Open:           { label: 'Open',            css: 'badge-pending' },
  Closed:         { label: 'Closed',          css: 'badge-expired' },
};

@Pipe({ name: 'statusLabel', standalone: true })
export class StatusLabelPipe implements PipeTransform {
  transform(status: string, mode: 'label' | 'css' = 'label'): string {
    const entry = STATUS_MAP[status];
    if (!entry) return status;
    return mode === 'css' ? entry.css : entry.label;
  }
}
