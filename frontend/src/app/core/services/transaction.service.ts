import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { StockTransaction, TransactionLine } from '../models/inventory.model';

interface BackendLine {
  itemId: number;
  itemCode: string;
  itemName: string;
  quantity: number;
  unitName: string;
  availableStock: number;
}

interface BackendTx {
  id: number;
  transactionNo: string;
  type: 'IN' | 'OUT' | 'ADJUST';
  warehouseId: number;
  warehouseName: string;
  transactionDate: string;
  referenceNo: string;
  notes?: string;
  createdBy: string;
  createdAt: string;
  status: 'Berhasil' | 'Dibatalkan';
  cancelledOfId?: number;
  cancellationReason?: string;
  lines: BackendLine[];
}

interface PagedTransactions {
  items: BackendTx[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly http = inject(HttpClient);

  readonly transactions = signal<StockTransaction[]>([]);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading.set(true);
    this.http.get<PagedTransactions>('/api/transactions?pageSize=100').subscribe({
      next: res => {
        const mapped: StockTransaction[] = res.items.map(t => ({
          id: t.id,
          transactionNo: t.transactionNo,
          type: t.type,
          warehouseId: t.warehouseId,
          warehouseName: t.warehouseName,
          transactionDate: t.transactionDate ? t.transactionDate.substring(0, 10) : '',
          referenceNo: t.referenceNo,
          notes: t.notes ?? '',
          createdBy: t.createdBy,
          createdAt: t.createdAt
            ? new Date(t.createdAt).toLocaleDateString('id-ID', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
              })
            : '',
          status: t.status,
          lines: t.lines.map(l => ({
            itemCode: l.itemCode,
            itemName: l.itemName,
            quantity: l.quantity,
            unitName: l.unitName,
            available: l.availableStock ?? 0,
          })),
        }));
        this.transactions.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  add(tx: Omit<StockTransaction, 'id'>, itemIds?: number[]): void {
    const linesPayload = tx.lines.map((l, idx) => ({
      itemId: itemIds && itemIds[idx] ? itemIds[idx] : 1,
      quantity: l.quantity,
    }));

    const payload = {
      warehouseId: tx.warehouseId,
      transactionDate: tx.transactionDate || new Date().toISOString(),
      referenceNo: tx.referenceNo,
      notes: tx.notes,
      lines: linesPayload,
    };

    const endpoint = tx.type === 'OUT' ? '/api/transactions/out' : '/api/transactions/in';

    this.http.post<BackendTx>(endpoint, payload).subscribe({
      next: () => this.loadTransactions(),
      error: () => {
        // Fallback optimis jika API belum siap
        this.transactions.update(list => [
          { ...tx, id: Math.max(0, ...list.map(t => t.id)) + 1 },
          ...list,
        ]);
      },
    });
  }

  cancelTransaction(id: number, reason: string): void {
    this.http
      .post(`/api/transactions/${id}/cancel`, { reason })
      .subscribe({
        next: () => this.loadTransactions(),
      });
  }
}
