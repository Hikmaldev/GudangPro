import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/services/inventory.service';
import { ToastService } from '../../core/services/toast.service';
import { Item } from '../../core/models/inventory.model';

@Component({
  selector: 'app-items',
  imports: [ReactiveFormsModule],
  templateUrl: './items.component.html',
  styleUrl: './items.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemsComponent {
  private readonly inventory = inject(InventoryService);
  protected readonly toast = inject(ToastService);

  readonly categories = this.inventory.categories;
  readonly units = this.inventory.units;
  readonly warehouses = this.inventory.warehouses;

  readonly items = computed(() =>
    this.inventory.items().filter(item => {
      if (!this.search().trim() && this.status() === 'Semua') return true;
      const q = this.search().trim().toLowerCase();
      const matchesSearch =
        !q || item.code.toLowerCase().includes(q) || item.name.toLowerCase().includes(q);
      const matchesStatus =
        this.status() === 'Semua' ||
        (this.status() === 'Aktif' ? item.isActive : !item.isActive);
      return matchesSearch && matchesStatus;
    }),
  );

  readonly totalItems = computed(() => this.inventory.items().length);
  readonly activeItems = computed(() => this.inventory.items().filter(i => i.isActive).length);
  readonly lowStockCount = computed(
    () => this.inventory.items().filter(i => i.quantity <= i.minStock).length,
  );
  readonly inactiveCount = computed(() => this.inventory.items().filter(i => !i.isActive).length);

  readonly search = signal('');
  readonly status = signal('Semua');
  readonly showForm = signal(false);
  readonly editing = signal<Item | null>(null);

  readonly form = new FormGroup({
    code: new FormControl('', [Validators.required]),
    name: new FormControl('', [Validators.required]),
    categoryName: new FormControl('Bahan Baku', [Validators.required]),
    unitName: new FormControl('Pcs', [Validators.required]),
    minStock: new FormControl(10, [Validators.required, Validators.min(0)]),
    warehouseName: new FormControl('Gudang Utama', [Validators.required]),
  });

  openAdd(): void {
    this.editing.set(null);
    this.form.reset({
      code: '',
      name: '',
      categoryName: this.categories()[0]?.name ?? 'Bahan Baku',
      unitName: this.units()[0]?.name ?? 'Pcs',
      minStock: 10,
      warehouseName: this.warehouses()[0]?.name ?? 'Gudang Utama',
    });
    this.showForm.set(true);
  }

  openEdit(item: Item): void {
    this.editing.set(item);
    this.form.setValue({
      code: item.code,
      name: item.name,
      categoryName: item.categoryName,
      unitName: item.unitName,
      minStock: item.minStock,
      warehouseName: item.warehouseName,
    });
    this.showForm.set(true);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const existing = this.editing();

    const cat = this.categories().find(c => c.name === v.categoryName);
    const unit = this.units().find(u => u.name === v.unitName);
    const categoryId = cat?.id ?? (existing?.categoryId || 1);
    const unitId = unit?.id ?? (existing?.unitId || 1);

    if (existing) {
      this.inventory.updateItem(existing.id, {
        name: v.name!,
        categoryId,
        unitId,
        minStock: v.minStock!,
      });
      this.toast.show(`Barang ${v.name} diperbarui`, 'success');
    } else {
      this.inventory.addItem({
        code: v.code!,
        name: v.name!,
        categoryId,
        categoryName: v.categoryName!,
        unitId,
        unitName: v.unitName!,
        minStock: v.minStock!,
        quantity: 0,
        isActive: true,
        warehouseName: v.warehouseName!,
      });
      this.toast.show(`Barang ${v.name} ditambahkan`, 'success');
    }
    this.showForm.set(false);
  }

  deactivate(item: Item): void {
    this.inventory.deactivateItem(item.id);
    this.toast.show(`${item.name} dinonaktifkan`, 'error');
  }

  exportStock(): void {
    window.open('/api/reports/export?type=stock', '_blank');
    this.toast.show('Mengunduh laporan stok CSV...', 'success');
  }
}
