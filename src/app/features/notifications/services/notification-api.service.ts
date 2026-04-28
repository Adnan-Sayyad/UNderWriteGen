import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Notification, NotificationStatus } from '../models/notification.model';
import { PagedResponse, ApiResponse } from '../../../shared/models/api-response.model';
import { PageRequest } from '../../../shared/models/pagination.model';

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly base = `${environment.apiBaseUrl}/notifications`;

  constructor(private http: HttpClient) {}

  getAll(req: PageRequest, status?: NotificationStatus) {
    const fromObj: Record<string, unknown> = { ...req };
    if (status) fromObj['status'] = status;
    return this.http.get<PagedResponse<Notification>>(this.base, { params: new HttpParams({ fromObject: fromObj as Record<string, string> }) });
  }
  markRead(id: string) { return this.http.post<ApiResponse<Notification>>(`${this.base}/${id}/read`, {}); }
  markAllRead() { return this.http.post<ApiResponse<void>>(`${this.base}/read-all`, {}); }
  dismiss(id: string) { return this.http.post<ApiResponse<Notification>>(`${this.base}/${id}/dismiss`, {}); }
  getUnreadCount() { return this.http.get<ApiResponse<number>>(`${this.base}/unread-count`); }
}
