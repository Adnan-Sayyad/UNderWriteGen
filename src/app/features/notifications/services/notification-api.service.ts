import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Notification } from '../models/notification.model';
import { ApiResponse } from '../../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly base = `${environment.apiBaseUrl}/notifications`;

  constructor(private http: HttpClient) {}

  // GET /api/notifications/my — returns notifications where the caller is sender or recipient
  getMy() {
    return this.http.get<ApiResponse<Notification[]>>(`${this.base}/my`);
  }

  // GET /api/notifications/unread-count
  getUnreadCount() {
    return this.http.get<ApiResponse<number>>(`${this.base}/unread-count`);
  }

  // GET /api/notifications/{id}
  getById(id: string) {
    return this.http.get<ApiResponse<Notification>>(`${this.base}/${id}`);
  }

  // PUT /api/notifications/{id}/read
  markRead(id: string) {
    return this.http.put<ApiResponse<string>>(`${this.base}/${id}/read`, {});
  }

  // PUT /api/notifications/{id}/dismiss
  dismiss(id: string) {
    return this.http.put<ApiResponse<string>>(`${this.base}/${id}/dismiss`, {});
  }

  // POST /api/notifications
  create(payload: { recipientEmail: string; message: string; category: string }) {
    return this.http.post<ApiResponse<Notification>>(this.base, payload);
  }

  // POST /api/notifications/broadcast
  broadcast(payload: { recipientGroup: string; message: string; category: string }) {
    return this.http.post<ApiResponse<{ sentCount: number }>>(`${this.base}/broadcast`, payload);
  }

  // DELETE /api/notifications/{id}
  delete(id: string) {
    return this.http.delete<ApiResponse<void>>(`${this.base}/${id}`);
  }
}
