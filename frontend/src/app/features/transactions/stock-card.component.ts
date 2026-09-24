import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { StockService } from '../../core/services/stock.service';
import { InventoryService } from '../../core/services/inventory.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-stock-card',
  templateUrl: './stock-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StockCardComponent {
  private readonly stock = inject(StockService);
  private readonly inventory = inject(InventoryService);
  protected readonly toast = inject(ToastService);

  readonly items = this.inventory.items;
  readonly warehouses = this.inventory.warehouses;
  readonly card = this.stock.card;

  readonly selectedItemId = signal<number>(1);
  readonly selectedWarehouseId = signal<number>(1);

  readonly status = computed(() => {
    const item = this.card().item;
    return item.quantity <= item.minStock
      ? 'Kritis'
      : item.quantity <= item.minStock * 1.5
        ? 'Menipis'
        : 'Aman';
  });

  onItemChange(itemId: number): void {
    this.selectedItemId.set(itemId);
    this.stock.loadCard(itemId, this.selectedWarehouseId());
  }

  onWarehouseChange(warehouseId: number): void {
    this.selectedWarehouseId.set(warehouseId);
    this.stock.loadCard(this.selectedItemId(), warehouseId);
  }

  exportPdf(): void {
    this.toast.show('Fitur ekspor PDF kartu stok sedang disiapkan', 'info');
  }
}
