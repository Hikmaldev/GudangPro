import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryStock, DashboardSummary, TrendPoint } from '../models/inventory.model';

interface BackendSummary {
  totalItems: number;
  transactionsToday: number;
  lowStockCount: number;
  stockValue: number;
  itemDelta: number;
  transactionDelta: number;
  valueDelta: number;
}

interface BackendCharts {
  categoryStock: {
    name: string;
    value: number;
    percentage: number;
    color: string;
  }[];
  trend: {
    label: string;
    incoming: number;
    outgoing: number;
  }[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  readonly summary = signal<DashboardSummary>({
    totalItems: 0,
    transactionsToday: 0,
    lowStockCount: 0,
    stockValue: 'Rp 0',
    itemDelta: 0,
    transactionDelta: 0,
    valueDelta: 0,
  });

  readonly categoryStock = signal<CategoryStock[]>([]);
  readonly trend = signal<TrendPoint[]>([]);

  constructor() {
    this.loadDashboard();
  }

  loadDashboard(warehouseId?: number): void {
    const q = warehouseId ? `?warehouseId=${warehouseId}` : '';

    this.http.get<BackendSummary>(`/api/dashboard/summary${q}`).subscribe({
      next: data => {
        let formattedValue = 'Rp 0';
        if (data.stockValue >= 1_000_000_000) {
          formattedValue = `Rp ${(data.stockValue / 1_000_000_000).toFixed(1)}M`;
        } else if (data.stockValue >= 1_000_000) {
          formattedValue = `Rp ${(data.stockValue / 1_000_000).toFixed(1)}Jt`;
        } else {
          formattedValue = `Rp ${data.stockValue.toLocaleString('id-ID')}`;
        }

        this.summary.set({
          totalItems: data.totalItems,
          transactionsToday: data.transactionsToday,
          lowStockCount: data.lowStockCount,
          stockValue: formattedValue,
          itemDelta: data.itemDelta,
          transactionDelta: data.transactionDelta,
          valueDelta: data.valueDelta,
        });
      },
      error: () => {},
    });

    this.http.get<BackendCharts>(`/api/dashboard/charts${q}`).subscribe({
      next: data => {
        this.categoryStock.set(data.categoryStock);
        this.trend.set(data.trend);
      },
      error: () => {},
    });
  }
}
