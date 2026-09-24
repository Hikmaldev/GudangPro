import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface UserSession {
  id?: number;
  username: string;
  fullName: string;
  role: 'Admin Gudang' | 'Staf Gudang' | 'Pemilik';
  isActive?: boolean;
  warehouseIds?: number[];
  lastLoginAt?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: UserSession;
}

/**
 * AuthService mengelola autentikasi pengguna dengan JWT bearer token.
 * Sesuai PRD §10.4:
 * - POST /api/auth/login (FR-AUTH-01)
 * - POST /api/auth/refresh (FR-AUTH-02)
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly accessToken = signal<string | null>(
    localStorage.getItem('accessToken'),
  );
  readonly currentUser = signal<UserSession | null>(
    JSON.parse(localStorage.getItem('user') ?? 'null'),
  );
  readonly isAuthenticated = computed(() => !!this.accessToken());

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/auth/login', { username, password })
      .pipe(tap(result => this.setSession(result)));
  }

  refresh(): Observable<LoginResponse> {
    const refreshToken = localStorage.getItem('refreshToken') ?? '';
    return this.http
      .post<LoginResponse>('/api/auth/refresh', { refreshToken })
      .pipe(tap(result => this.setSession(result)));
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.accessToken.set(null);
    this.currentUser.set(null);
    void this.router.navigate(['/login']);
  }

  private setSession(result: LoginResponse): void {
    localStorage.setItem('accessToken', result.accessToken);
    localStorage.setItem('refreshToken', result.refreshToken);
    localStorage.setItem('user', JSON.stringify(result.user));
    this.accessToken.set(result.accessToken);
    this.currentUser.set(result.user);
  }
}
