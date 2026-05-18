import { Component, Input, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface Breadcrumb { label: string; route?: string; }

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './page-header.html',
})
export class PageHeader {
  @Input({ required: true }) title!: string;
  @Input() subtitle?: string;
  @Input() icon?: string;

  /** Strip the generic "Home → /" crumb — each role has its own landing page. */
  private _breadcrumbs: Breadcrumb[] = [];
  get visibleCrumbs(): Breadcrumb[] { return this._breadcrumbs; }

  @Input()
  set breadcrumbs(val: Breadcrumb[]) {
    this._breadcrumbs = (val ?? []).filter(c => !(c.label === 'Home' && c.route === '/'));
  }
}
