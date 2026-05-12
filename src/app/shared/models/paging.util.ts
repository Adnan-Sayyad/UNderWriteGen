import { PagedResponse } from './api-response.model';
import { PageRequest } from './pagination.model';

/**
 * Build a PagedResponse<T> from either a server-paged envelope ({content,totalPages,…})
 * or a flat array. When the backend returns a flat array, the items are sliced
 * client-side according to `req.page` and `req.size` so the shared Pagination
 * component still works correctly.
 */
export function toPagedResponse<T>(
  res: any,
  req: PageRequest,
  normalize: (r: any) => T,
): PagedResponse<T> {
  // Server already returned a paged envelope — trust its metadata.
  if (res && !Array.isArray(res) && (res.content || res.data) && (res.totalPages != null || res.totalElements != null)) {
    const items: any[] = (res.content ?? res.data ?? []).map(normalize);
    return {
      content: items, data: items,
      totalPages:    res.totalPages    ?? Math.max(1, Math.ceil((res.totalElements ?? items.length) / (req.size || 10))),
      totalElements: res.totalElements ?? items.length,
      size:          req.size ?? 10,
      number:        req.page ?? 0,
    } as unknown as PagedResponse<T>;
  }

  // Otherwise we got either a raw array or a plain wrapper — slice client-side.
  const raw: any[] = Array.isArray(res) ? res : (res?.data ?? res?.content ?? []);
  const all = raw.map(normalize);
  const size = req.size && req.size > 0 ? req.size : 10;
  const page = Math.max(0, req.page ?? 0);
  const totalElements = all.length;
  const totalPages    = Math.max(1, Math.ceil(totalElements / size));
  const start = Math.min(page, totalPages - 1) * size;
  const slice = all.slice(start, start + size);

  return {
    content: slice, data: slice,
    totalPages, totalElements,
    size, number: page,
  } as unknown as PagedResponse<T>;
}
