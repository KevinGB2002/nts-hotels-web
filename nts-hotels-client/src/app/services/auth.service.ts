import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.model';
import { User } from '../models/user.model';

const TOKEN_KEY = 'nts_token';
const EXPIRES_KEY = 'nts_expires_at';
const USER_KEY = 'nts_user';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly currentUser = signal<User | null>(this.readUser());

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, credentials).pipe(
      tap(res => {
        localStorage.setItem(TOKEN_KEY, res.token);
        localStorage.setItem(EXPIRES_KEY, res.expiresAt);
        localStorage.setItem(USER_KEY, JSON.stringify(res.user));
        this.currentUser.set(res.user);
      })
    );
  }

  logout(): void {
    [TOKEN_KEY, EXPIRES_KEY, USER_KEY].forEach(k => localStorage.removeItem(k));
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    if (!this.getToken()) return false;
    const expires = localStorage.getItem(EXPIRES_KEY);
    return !expires || new Date(expires).getTime() > Date.now();
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  getRole(): string {
    const claims = this.decodeToken();
    const raw = claims?.['role'] ?? claims?.[ROLE_CLAIM] ?? this.currentUser()?.roleName ?? '';
    return (Array.isArray(raw) ? raw[0] : String(raw)).toLowerCase();
  }

  isAdmin(): boolean {
    const role = this.getRole();
    return this.isAuthenticated() && (role === 'admin' || role === 'administrador');
  }

  private decodeToken(): Record<string, unknown> | null {
    const payload = this.getToken()?.split('.')[1];
    if (!payload) return null;
    try {
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decodeURIComponent(escape(json)));
    } catch {
      return null;
    }
  }

  private readUser(): User | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as User) : null;
    } catch {
      return null;
    }
  }
}
