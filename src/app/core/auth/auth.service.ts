import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface LoginPayload { email: string; password: string; }
export interface AuthUser { userId: string; name: string; email: string; role: string; }
export interface AuthResponse { token: string; refreshToken: string; user: AuthUser; }

interface BackendAuthDto {
  userId: string;
  email: string;
  role: string;
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  refreshTokenExpiry: string;
}
interface BackendApiResponse<T> { success: boolean; message: string; data: T; }

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber?: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiBaseUrl}/auth`;

  readonly currentUser = signal<AuthUser | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(payload: LoginPayload) {
    return this.http
      .post<BackendApiResponse<BackendAuthDto>>(`${this.api}/login`, {
        email: payload.email,
        password: payload.password,
      })
      .pipe(
        map(res => {
          if (!res.success || !res.data) throw new Error(res.message || 'Login failed');
          return res.data;
        }),
        tap(dto => {
          const user: AuthUser = {
            userId: dto.userId,
            name: dto.email.split('@')[0],
            email: dto.email,
            role: dto.role,
          };
          localStorage.setItem(environment.tokenKey, dto.accessToken);
          localStorage.setItem(environment.refreshTokenKey, dto.refreshToken);
          localStorage.setItem('uwpro_user', JSON.stringify(user));
          this.currentUser.set(user);
        })
      );
  }

  refreshToken() {
    const token = this.getToken();
    const refresh = localStorage.getItem(environment.refreshTokenKey);
    if (!token || !refresh) return;
    return this.http
      .post<BackendApiResponse<BackendAuthDto>>(`${this.api}/refresh-token`, {
        accessToken: token,
        refreshToken: refresh,
      })
      .pipe(
        map(res => {
          if (!res.success || !res.data) throw new Error('Token refresh failed');
          return res.data;
        }),
        tap(dto => {
          localStorage.setItem(environment.tokenKey, dto.accessToken);
          localStorage.setItem(environment.refreshTokenKey, dto.refreshToken);
        })
      );
  }

  register(payload: RegisterPayload) {
    return this.http.post<{ success: boolean; message: string; data: any }>(
      `${this.api}/register`, payload
    );
  }

  changePassword(payload: ChangePasswordPayload) {
    return this.http.post<BackendApiResponse<null>>(`${this.api}/change-password`, payload);
  }

  logout(): void {
    const userId = this.currentUser()?.userId;
    if (userId) {
      this.http.post(`${this.api}/logout`, { userId }).subscribe({ error: () => {} });
    }
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.refreshTokenKey);
    localStorage.removeItem('uwpro_user');
    localStorage.removeItem('uwpro_my_agent_id');   // clear agent cache on logout
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  updateStoredName(name: string): void {
    const user = this.currentUser();
    if (user) {
      const updated = { ...user, name };
      localStorage.setItem('uwpro_user', JSON.stringify(updated));
      this.currentUser.set(updated);
    }
  }

  updateStoredProfile(firstName: string, lastName: string, email: string): void {
    const user = this.currentUser();
    if (user) {
      const updated = { ...user, name: `${firstName} ${lastName}`, email };
      localStorage.setItem('uwpro_user', JSON.stringify(updated));
      this.currentUser.set(updated);
    }
  }

  getToken(): string | null {
    return localStorage.getItem(environment.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(...roles: string[]): boolean {
    const user = this.currentUser();
    return !!user && roles.includes(user.role);
  }

  private loadUser(): AuthUser | null {
    try {
      const raw = localStorage.getItem('uwpro_user');
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
