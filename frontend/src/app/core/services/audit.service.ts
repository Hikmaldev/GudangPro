import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuditLog } from '../models/inventory.model';

interface BackendLog {
  id: number;
  userFullName: string;
  username: string;
  action: string;
  entityName: string;
  entityId?: string;
  detail?: string;
  ipAddress?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);

  readonly logs = signal<AuditLog[]>([]);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.loadLogs();
  }

  loadLogs(action?: string, search?: string): void {
    this.loading.set(true);
    let params = '?limit=100';
    if (action && action !== 'Semua') params += `&action=${encodeURIComponent(action)}`;
    if (search) params += `&search=${encodeURIComponent(search)}`;

    this.http.get<BackendLog[]>(`/api/audit-logs${params}`).subscribe({
      next: data => {
        const mapped: AuditLog[] = data.map(l => ({
          id: l.id,
          userFullName: l.userFullName,
          username: l.username,
          action: l.action,
          entityName: l.entityName,
          detail: l.detail ?? `${l.action} ${l.entityName}`,
          ipAddress: l.ipAddress ?? '127.0.0.1',
          createdAt: l.createdAt
            ? new Date(l.createdAt).toLocaleDateString('id-ID', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
              })
            : '',
        }));
        this.logs.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
