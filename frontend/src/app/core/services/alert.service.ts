import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Alert, AlertLevel } from '../models/inventory.model';

interface BackendAlert {
  id: number;
  itemId: number;
  itemCode: string;
  itemName: string;
  warehouseId: number;
  warehouseName: string;
  message: string;
  level: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AlertService {
  private readonly http = inject(HttpClient);

  readonly alerts = signal<Alert[]>([]);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.loadAlerts();
  }

  loadAlerts(filter?: string): void {
    this.loading.set(true);
    const q = filter ? `?filter=${encodeURIComponent(filter)}` : '';
    this.http.get<BackendAlert[]>(`/api/alerts${q}`).subscribe({
      next: data => {
        const mapped: Alert[] = data.map(a => ({
          id: a.id,
          itemId: a.itemId,
          warehouseName: a.warehouseName,
          itemName: a.itemName,
          message: a.message,
          level: (a.level === 'kritis' ? 'kritis' : 'menipis') as AlertLevel,
          isRead: a.isRead,
          createdAt: a.createdAt
            ? new Date(a.createdAt).toLocaleDateString('id-ID', {
                day: 'numeric',
                month: 'short',
                hour: '2-digit',
                minute: '2-digit',
              })
            : '',
        }));
        this.alerts.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  markRead(id: number): void {
    this.http.patch(`/api/alerts/${id}/read`, {}).subscribe({
      next: () => {
        this.alerts.update(list =>
          list.map(alert => (alert.id === id ? { ...alert, isRead: true } : alert)),
        );
      },
    });
  }

  markAllRead(): void {
    this.http.post('/api/alerts/mark-all-read', {}).subscribe({
      next: () => {
        this.alerts.update(list => list.map(alert => ({ ...alert, isRead: true })));
      },
    });
  }
}
