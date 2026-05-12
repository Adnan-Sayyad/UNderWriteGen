import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pager.html',
  styleUrl: './pager.css',
})
export class Pager {
  @Input() set page(v: number)          { this._page.set(v); }
  @Input() set size(v: number)          { this._size.set(v); }
  @Input() set totalElements(v: number) { this._total.set(v); }
  @Input() set totalPages(v: number)    { this._totalPages.set(v); }
  @Input() pageSizes: number[] = [10, 20, 50, 100];
  @Input() disabled = false;

  @Output() pageChange = new EventEmitter<number>();
  @Output() sizeChange = new EventEmitter<number>();

  readonly _page       = signal(0);
  readonly _size       = signal(20);
  readonly _total      = signal(0);
  readonly _totalPages = signal(0);

  readonly hasPrev = computed(() => this._page() > 0 && !this.disabled);
  readonly hasNext = computed(() => this._page() < this._totalPages() - 1 && !this.disabled);

  readonly fromIndex = computed(() =>
    this._total() === 0 ? 0 : this._page() * this._size() + 1
  );
  readonly toIndex = computed(() =>
    Math.min(this._total(), (this._page() + 1) * this._size())
  );

  /** Render a windowed page list e.g. 1 … 4 5 [6] 7 8 … 12 */
  readonly visiblePages = computed<(number | '…')[]>(() => {
    const total = this._totalPages();
    const cur   = this._page();
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i);
    }
    const pages: (number | '…')[] = [0];
    const left  = Math.max(1, cur - 1);
    const right = Math.min(total - 2, cur + 1);
    if (left > 1) pages.push('…');
    for (let i = left; i <= right; i++) pages.push(i);
    if (right < total - 2) pages.push('…');
    pages.push(total - 1);
    return pages;
  });

  goTo(p: number): void {
    if (this.disabled || p === this._page() || p < 0 || p >= this._totalPages()) return;
    this.pageChange.emit(p);
  }

  prev(): void { if (this.hasPrev()) this.pageChange.emit(this._page() - 1); }
  next(): void { if (this.hasNext()) this.pageChange.emit(this._page() + 1); }
  first(): void { if (this.hasPrev()) this.pageChange.emit(0); }
  last(): void  { if (this.hasNext()) this.pageChange.emit(this._totalPages() - 1); }

  changeSize(newSize: number): void {
    const n = Number(newSize);
    if (!isNaN(n) && n > 0 && n !== this._size()) this.sizeChange.emit(n);
  }
}
