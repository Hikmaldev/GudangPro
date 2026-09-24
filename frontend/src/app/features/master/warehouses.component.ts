import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/services/inventory.service';
import { ToastService } from '../../core/services/toast.service';
import { Warehouse } from '../../core/models/inventory.model';

@Component({
  selector: 'app-warehouses',
  imports: [ReactiveFormsModule],
  templateUrl: './warehouses.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WarehousesComponent {
  private readonly inventory = inject(InventoryService);
  private readonly toast = inject(ToastService);
  readonly warehouses = this.inventory.warehouses;
  readonly totalWarehouses = computed(() => this.warehouses().length);
  readonly activeWarehouses = computed(() => this.warehouses().filter(w => w.isActive).length);
  readonly totalItems = computed(() => this.inventory.items().length);
  readonly totalStock = computed(() => this.inventory.items().reduce((sum, item) => sum + item.quantity, 0));

  readonly search = signal('');
  readonly showForm = signal(false);
  readonly form = new FormGroup({
    code: new FormControl('', [Validators.required]),
    name: new FormControl('', [Validators.required]),
    address: new FormControl('', [Validators.required]),
  });

  filtered(): Warehouse[] {
    const q = this.search().trim().toLowerCase();
    return q ? this.warehouses().filter(w => w.code.toLowerCase().includes(q) || w.name.toLowerCase().includes(q)) : this.warehouses();
  }

  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    this.inventory.addWarehouse({ code: v.code!, name: v.name!, address: v.address!, isActive: true, itemCount: 0 });
    this.toast.show(`Gudang ${v.name} ditambahkan`, 'success');
    this.showForm.set(false);
    this.form.reset();
  }
}