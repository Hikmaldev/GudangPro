import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Role, User } from '../models/inventory.model';

interface BackendUser {
  id: number;
  username: string;
  fullName: string;
  role: string;
  isActive: boolean;
  warehouseIds: number[];
  lastLoginAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);

  readonly users = signal<User[]>([]);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.http.get<BackendUser[]>('/api/users').subscribe({
      next: data => {
        const mapped: User[] = data.map(u => ({
          id: u.id,
          username: u.username,
          fullName: u.fullName,
          role: u.role as Role,
          isActive: u.isActive,
          warehouseIds: u.warehouseIds ?? [],
          lastLoginAt: u.lastLoginAt
            ? new Date(u.lastLoginAt).toLocaleDateString('id-ID', {
                day: 'numeric',
                month: 'short',
                hour: '2-digit',
                minute: '2-digit',
              })
            : 'Belum pernah',
        }));
        this.users.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  add(user: Omit<User, 'id'>, password = ''): void {
    const payload = {
      username: user.username,
      password: password || `${user.username}123`,
      fullName: user.fullName,
      role: user.role,
      warehouseIds: user.warehouseIds,
    };

    this.http.post<BackendUser>('/api/users', payload).subscribe({
      next: () => this.loadUsers(),
      error: () => {
        // Fallback optimis jika API belum siap
        this.users.update(list => [
          ...list,
          { ...user, id: Math.max(0, ...list.map(u => u.id)) + 1 },
        ]);
      },
    });
  }

  resetPassword(id: number, newPassword: string): void {
    this.http.post(`/api/users/${id}/reset-password`, { newPassword }).subscribe();
  }
}
