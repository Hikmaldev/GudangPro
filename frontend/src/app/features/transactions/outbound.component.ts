import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/services/inventory.service';
import { TransactionService } from '../../core/services/transaction.service';
import { ToastService } from '../../core/services/toast.service';

type OutboundLine = { itemId: FormControl<string>; quantity: FormControl<number> };

@Component({
  selector: 'app-outbound',
  imports: [ReactiveFormsModule],
  templateUrl: './outbound.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OutboundComponent {
  private readonly inventory = inject(InventoryService);
  private readonly transactions = inject(TransactionService);
  protected readonly toast = inject(ToastService);
  readonly items = this.inventory.items;
  readonly warehouses = this.inventory.warehouses;
  readonly todayCount = computed(() => this.transactions.transactions().filter(t => t.type === 'OUT').length);

  readonly form = new FormGroup({
    warehouseId: new FormControl(1, [Validators.required]),
    date: new FormControl(new Date().toISOString().substring(0, 10), [Validators.required]),
    reference: new FormControl('', [Validators.required]),
    receiver: new FormControl(''),
    notes: new FormControl(''),
    lines: new FormArray<FormGroup<OutboundLine>>([]),
  });

  get lines(): FormArray<FormGroup<OutboundLine>> {
    return this.form.get('lines') as FormArray;
  }
  readonly warning = signal('');

  addLine(): void {
    this.lines.push(
      new FormGroup<OutboundLine>({
        itemId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        quantity: new FormControl(1, {
          nonNullable: true,
          validators: [Validators.required, Validators.min(1)],
        }),
      }),
    );
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  resetForm(): void {
    this.form.reset({
      warehouseId: 1,
      date: new Date().toISOString().substring(0, 10),
      reference: '',
      receiver: '',
      notes: '',
    });
    while (this.lines.length) {
      this.lines.removeAt(0);
    }
    this.warning.set('');
    this.toast.show('Form dikosongkan', 'info');
  }

  availableFor(index: number): number {
    const line = this.lines.at(index) as FormGroup;
    const itemId = Number(line.get('itemId')?.value);
    return this.items().find(i => i.id === itemId)?.quantity ?? 0;
  }

  unitFor(index: number): string {
    const line = this.lines.at(index) as FormGroup;
    const itemId = Number(line.get('itemId')?.value);
    return this.items().find(i => i.id === itemId)?.unitName ?? '';
  }

  isUnderMinimum(index: number): boolean {
    const line = this.lines.at(index) as FormGroup;
    const itemId = Number(line.get('itemId')?.value);
    const qty = Number(line.get('quantity')?.value) || 0;
    const item = this.items().find(i => i.id === itemId);
    return !!item && item.quantity - qty <= item.minStock;
  }

  insufficient(index: number): boolean {
    const line = this.lines.at(index) as FormGroup;
    const qty = Number(line.get('quantity')?.value) || 0;
    return qty > 0 && qty > this.availableFor(index);
  }

  save(): void {
    if (this.lines.length === 0) {
      this.toast.show('Tambahkan minimal 1 barang', 'error');
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    // Validasi stok (FR-OUT-02): tolak jika melebihi stok tersedia.
    for (let i = 0; i < this.lines.length; i++) {
      const line = this.lines.at(i) as FormGroup;
      const itemId = Number(line.get('itemId')?.value);
      const qty = Number(line.get('quantity')?.value);
      const item = this.items().find(x => x.id === itemId);
      if (item && qty > item.quantity) {
        this.warning.set(
          `Stok ${item.name} tidak cukup. Tersedia ${item.quantity} ${item.unitName}.`,
        );
        this.toast.show(this.warning(), 'error');
        return;
      }
    }
    const wh = this.warehouses().find(w => w.id === Number(v.warehouseId));
    this.transactions.add(
      {
        transactionNo: `#TRX-${new Date().toISOString().substring(0, 10).replaceAll('-', '').substring(2)}-${String(100 + this.transactions.transactions().length).substring(1)}`,
        type: 'OUT',
        warehouseId: Number(v.warehouseId),
        warehouseName: wh?.name ?? 'Gudang Utama',
        transactionDate: v.date ?? '',
        referenceNo: v.reference ?? '',
        notes: v.notes ?? '',
        createdBy: 'Budi Santoso',
        createdAt: new Date().toLocaleTimeString('id-ID', { hour: '2-digit', minute: '2-digit' }),
        status: 'Berhasil',
        lines: v.lines.map((line: { itemId: string; quantity: number }) => {
          const item = this.items().find(i => i.id === Number(line.itemId));
          return {
            itemCode: item?.code ?? '',
            itemName: item?.name ?? '',
            quantity: line.quantity,
            unitName: item?.unitName ?? '',
            available: item?.quantity ?? 0,
          };
        }),
      },
      v.lines.map((line: { itemId: string }) => Number(line.itemId)),
    );
    this.inventory.items.update(list =>
      list.map(item => {
        const line = v.lines.find((l: { itemId: string }) => Number(l.itemId) === item.id);
        return line ? { ...item, quantity: Math.max(0, item.quantity - line.quantity) } : item;
      }),
    );
    this.warning.set('');
    this.toast.show('Transaksi barang keluar berhasil disimpan', 'success');
    this.form.reset({
      warehouseId: 1,
      date: new Date().toISOString().substring(0, 10),
      reference: '',
      receiver: '',
      notes: '',
    });
    while (this.lines.length) {
      this.lines.removeAt(0);
    }
  }
}
