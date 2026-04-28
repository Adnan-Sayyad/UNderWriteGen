import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface LoginPayload { email: string; password: string; }
export interface AuthUser { userId: string; name: string; email: string; role: string; }
export interface AuthResponse { token: string; refreshToken: string; user: AuthUser; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = `${environment.apiBaseUrl}/auth`;

  readonly currentUser = signal<AuthUser | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  login(payload: LoginPayload) {
    return this.http.post<AuthResponse>(`${this.api}/login`, payload).pipe(
      tap(res => {
        localStorage.setItem(environment.tokenKey, res.token);
        localStorage.setItem(environment.refreshTokenKey, res.refreshToken);
        localStorage.setItem('uwpro_user', JSON.stringify(res.user));
        this.currentUser.set(res.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.refreshTokenKey);
    localStorage.removeItem('uwpro_user');
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
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
