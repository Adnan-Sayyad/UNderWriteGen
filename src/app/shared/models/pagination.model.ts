export interface PageRequest {
  page: number;
  size: number;
  sort?: string;
  direction?: 'asc' | 'desc';
}

export interface PageState {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export const DEFAULT_PAGE_REQUEST: PageRequest = { page: 0, size: 20, sort: 'createdDate', direction: 'desc' };
