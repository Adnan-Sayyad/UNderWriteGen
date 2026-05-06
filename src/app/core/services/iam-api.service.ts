import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';

export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  phoneNumber?: string;
  role: string;
  status: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AuditLogDto {
  id: string;
  userId?: string;
  email: string;
  action: string;
  resource: string;
  createdAt: string;
  metadata?: string;
  isDeleted: boolean;
}

export interface RegisterUserPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber?: string;
}

export interface AssignRolePayload {
  adminId: string;
  userId: string;
  roles: string[];
}

export interface UpdateUserPayload {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
}

export interface UpdateUserStatusPayload {
  status: 'Active' | 'Locked' | 'Disabled';
}

interface ApiResp<T> { success: boolean; message: string; data: T; }

@Injectable({ providedIn: 'root' })
export class IamApiService {
  private readonly usersBase = `${environment.apiBaseUrl}/users`;
  private readonly authBase  = `${environment.apiBaseUrl}/auth`;
  private readonly auditBase = `${environment.apiBaseUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  getUsers(adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .get<ApiResp<UserDto[]>>(this.usersBase, { params })
      .pipe(map(r => r.data ?? []));
  }

  getUserById(userId: string, adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .get<ApiResp<UserDto>>(`${this.usersBase}/${userId}`, { params })
      .pipe(map(r => r.data));
  }

  /** Self-profile — no admin required, any active user */
  getMyProfile(userId: string) {
    const params = new HttpParams().set('userId', userId);
    return this.http
      .get<ApiResp<UserDto>>(`${this.usersBase}/me`, { params })
      .pipe(map(r => r.data));
  }

  registerUser(payload: RegisterUserPayload) {
    return this.http
      .post<ApiResp<UserDto>>(`${this.authBase}/register`, payload)
      .pipe(map(r => r.data));
  }

  assignRole(payload: AssignRolePayload) {
    return this.http
      .post<ApiResp<UserDto>>(`${this.authBase}/assign-role`, payload)
      .pipe(map(r => r.data));
  }

  updateUser(userId: string, adminId: string, payload: UpdateUserPayload) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .put<ApiResp<UserDto>>(`${this.usersBase}/${userId}`, payload, { params })
      .pipe(map(r => r.data));
  }

  updateUserStatus(userId: string, adminId: string, payload: UpdateUserStatusPayload) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .patch<ApiResp<null>>(`${this.usersBase}/${userId}/status`, payload, { params })
      .pipe(map(r => r));
  }

  deleteUser(userId: string, adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .delete<ApiResp<null>>(`${this.usersBase}/${userId}`, { params })
      .pipe(map(r => r));
  }

  getAuditLogs(adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .get<ApiResp<AuditLogDto[]>>(this.auditBase, { params })
      .pipe(map(r => r.data ?? []));
  }

  getAuditLogsByUser(userId: string, adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .get<ApiResp<AuditLogDto[]>>(`${this.auditBase}/user/${userId}`, { params })
      .pipe(map(r => r.data ?? []));
  }

  getAuditLogsByResource(resource: string, adminId: string) {
    const params = new HttpParams().set('adminId', adminId);
    return this.http
      .get<ApiResp<AuditLogDto[]>>(`${this.auditBase}/resource/${resource}`, { params })
      .pipe(map(r => r.data ?? []));
  }
}
