import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { StockCard } from '../models/inventory.model';

interface BackendStockCard {
  item: {
    id: number;
    code: string;
    name: string;
    categoryId: number;
    categoryName: string;
    unitId: number;
    unitName: string;
    minStock: number;
    totalQuantity: number;
    isActive: boolean;
  };
  warehouseId: number;
  warehouseName: string;
  openingBalance: number;
  totalIncoming: number;
  totalOutgoing: number;
  closingBalance: number;
  entries: {
    date: string;
    transactionNo: string;
    reference: string;
    incoming: number | null;
    outgoing: number | null;
    balance: number;
    createdBy: string;
  }[];
}

const DEFAULT_CARD: StockCard = {
  item: {
    id: 1,
    code: 'BB-RS-001',
    name: 'Biji Plastik PP Grade A',
    categoryId: 1,
    categoryName: 'Bahan Baku',
    unitId: 2,
    unitName: 'Kg',
    minStock: 100,
    quantity: 450,
    isActive: true,
    warehouseName: 'Gudang Utama',
  },
  openingBalance: 250,
  totalIncoming: 200,
  totalOutgoing: 0,
  closingBalance: 450,
  entries: [],
};

@Injectable({ providedIn: 'root' })
export class StockService {
  private readonly http = inject(HttpClient);
  readonly card = signal<StockCard>(DEFAULT_CARD);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.loadCard(1, 1);
  }

  loadCard(itemId = 1, warehouseId = 1): void {
    this.loading.set(true);
    this.http
      .get<BackendStockCard>(`/api/stock/${itemId}/card?warehouseId=${warehouseId}`)
      .subscribe({
        next: data => {
          const mapped: StockCard = {
            item: {
              id: data.item.id,
              code: data.item.code,
              name: data.item.name,
              categoryId: data.item.categoryId,
              categoryName: data.item.categoryName,
              unitId: data.item.unitId,
              unitName: data.item.unitName,
              minStock: data.item.minStock,
              quantity: data.item.totalQuantity,
              isActive: data.item.isActive,
              warehouseName: data.warehouseName,
            },
            openingBalance: data.openingBalance,
            totalIncoming: data.totalIncoming,
            totalOutgoing: data.totalOutgoing,
            closingBalance: data.closingBalance,
            entries: data.entries.map(e => ({
              date: e.date
                ? new Date(e.date).toLocaleDateString('id-ID', {
                    day: 'numeric',
                    month: 'short',
                    year: 'numeric',
                  })
                : '',
              transactionNo: e.transactionNo,
              reference: e.reference,
              incoming: e.incoming ?? 0,
              outgoing: e.outgoing ?? 0,
              balance: e.balance,
              createdBy: e.createdBy,
            })),
          };
          this.card.set(mapped);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }
}
