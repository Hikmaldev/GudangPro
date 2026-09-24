import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category, Item, Unit, Warehouse } from '../models/inventory.model';

interface BackendItem {
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
  stockByWarehouse?: Record<number, number>;
}

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);

  readonly items = signal<Item[]>([]);
  readonly warehouses = signal<Warehouse[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly units = signal<Unit[]>([]);
  readonly loading = signal<boolean>(false);

  constructor() {
    this.refreshAll();
  }

  refreshAll(): void {
    this.loadItems();
    this.loadWarehouses();
    this.loadCategories();
    this.loadUnits();
  }

  loadItems(): void {
    this.loading.set(true);
    this.http.get<BackendItem[]>('/api/items').subscribe({
      next: data => {
        const mapped: Item[] = data.map(i => ({
          id: i.id,
          code: i.code,
          name: i.name,
          categoryId: i.categoryId,
          categoryName: i.categoryName,
          unitId: i.unitId,
          unitName: i.unitName,
          minStock: i.minStock,
          quantity: i.totalQuantity,
          isActive: i.isActive,
          warehouseName: 'Semua Gudang',
        }));
        this.items.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  loadWarehouses(): void {
    this.http.get<Warehouse[]>('/api/warehouses').subscribe({
      next: data => this.warehouses.set(data),
      error: () => {},
    });
  }

  loadCategories(): void {
    this.http.get<Category[]>('/api/categories').subscribe({
      next: data => this.categories.set(data),
      error: () => {},
    });
  }

  loadUnits(): void {
    this.http.get<Unit[]>('/api/units').subscribe({
      next: data => this.units.set(data),
      error: () => {},
    });
  }

  addItem(item: Omit<Item, 'id'>, callback?: (saved: Item) => void): void {
    const payload = {
      code: item.code,
      name: item.name,
      categoryId: item.categoryId || 1,
      unitId: item.unitId || 1,
      minStock: item.minStock,
    };
    this.http.post<BackendItem>('/api/items', payload).subscribe({
      next: saved => {
        this.loadItems();
        if (callback) {
          callback({
            id: saved.id,
            code: saved.code,
            name: saved.name,
            categoryId: saved.categoryId,
            categoryName: saved.categoryName,
            unitId: saved.unitId,
            unitName: saved.unitName,
            minStock: saved.minStock,
            quantity: saved.totalQuantity,
            isActive: saved.isActive,
            warehouseName: 'Semua Gudang',
          });
        }
      },
      error: () => {
        // Fallback optimis jika API belum siap
        this.items.update(list => [
          ...list,
          { ...item, id: Math.max(0, ...list.map(i => i.id)) + 1 },
        ]);
      },
    });
  }

  updateItem(
    id: number,
    data: { name: string; categoryId: number; unitId: number; minStock: number },
  ): void {
    this.http.put<BackendItem>(`/api/items/${id}`, data).subscribe({
      next: () => this.loadItems(),
      error: () => {
        this.items.update(list =>
          list.map(i => (i.id === id ? { ...i, name: data.name, minStock: data.minStock } : i)),
        );
      },
    });
  }

  deactivateItem(id: number): void {
    this.http.patch(`/api/items/${id}/deactivate`, {}).subscribe({
      next: () => this.loadItems(),
      error: () => {
        this.items.update(list =>
          list.map(item => (item.id === id ? { ...item, isActive: false } : item)),
        );
      },
    });
  }

  addWarehouse(warehouse: Omit<Warehouse, 'id'>): void {
    const payload = {
      code: warehouse.code,
      name: warehouse.name,
      address: warehouse.address,
    };
    this.http.post<Warehouse>('/api/warehouses', payload).subscribe({
      next: () => this.loadWarehouses(),
      error: () => {
        this.warehouses.update(list => [
          ...list,
          { ...warehouse, id: Math.max(0, ...list.map(w => w.id)) + 1 },
        ]);
      },
    });
  }
}
