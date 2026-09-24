import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/services/inventory.service';
import { TransactionService } from '../../core/services/transaction.service';
import { ToastService } from '../../core/services/toast.service';

interface InboundLine {
  itemId: string;
  quantity: number;
  note: string;
}

@Component({
  selector: 'app-inbound',
  imports: [ReactiveFormsModule],
  templateUrl: './inbound.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InboundComponent {
  private readonly inventory = inject(InventoryService);
  private readonly transactions = inject(TransactionService);
  protected readonly toast = inject(ToastService);
  readonly items = this.inventory.items;
  readonly warehouses = this.inventory.warehouses;
  readonly todayCount = computed(() => this.transactions.transactions().filter(t => t.type === 'IN').length);
  readonly todayTotal = computed(() =>
    this.transactions
      .transactions()
      .filter(t => t.type === 'IN')
      .reduce((sum, t) => sum + t.lines.reduce((s, l) => s + l.quantity, 0), 0),
  );

  readonly form = new FormGroup({
    warehouseId: new FormControl(1, [Validators.required]),
    date: new FormControl(new Date().toISOString().substring(0, 10), [Validators.required]),
    reference: new FormControl('', [Validators.required]),
    supplier: new FormControl(''),
    notes: new FormControl(''),
    lines: new FormArray<FormGroup<{ itemId: FormControl<string>; quantity: FormControl<number>; note: FormControl<string> }>>([]),
  });

  get lines(): FormArray<FormGroup<{ itemId: FormControl<string>; quantity: FormControl<number>; note: FormControl<string> }>> {
    return this.form.get('lines') as FormArray;
  }

  addLine(): void {
    this.lines.push(new FormGroup({
      itemId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      quantity: new FormControl(1, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
      note: new FormControl('', { nonNullable: true }),
    }));
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  resetForm(): void {
    this.form.reset({ warehouseId: 1, date: new Date().toISOString().substring(0, 10), reference: '', supplier: '', notes: '' });
    while (this.lines.length) { this.lines.removeAt(0); }
    this.toast.show('Form dikosongkan', 'info');
  }

  selectedUnit(index: number): string {
    const line = this.lines.at(index) as FormGroup;
    const itemId = Number(line.get('itemId')?.value);
    return this.items().find(i => i.id === itemId)?.unitName ?? '';
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
    const wh = this.warehouses().find(w => w.id === Number(v.warehouseId));
    this.transactions.add(
      {
        transactionNo: `#TRX-${new Date().toISOString().substring(0, 10).replaceAll('-', '').substring(2)}-${String(100 + this.transactions.transactions().length).substring(1)}`,
        type: 'IN',
        warehouseId: Number(v.warehouseId),
        warehouseName: wh?.name ?? 'Gudang Utama',
        transactionDate: v.date ?? '',
        referenceNo: v.reference ?? '',
        notes: v.notes ?? '',
        createdBy: 'Budi Santoso',
        createdAt: new Date().toLocaleTimeString('id-ID', { hour: '2-digit', minute: '2-digit' }),
        status: 'Berhasil',
        lines: v.lines.map((line: { itemId: string; quantity: number; note: string }) => {
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
    this.inventory.items.update(list => list.map(item => {
      const line = v.lines.find((l: { itemId: string }) => Number(l.itemId) === item.id);
      return line ? { ...item, quantity: item.quantity + line.quantity } : item;
    }));
    this.toast.show('Transaksi barang masuk berhasil disimpan', 'success');
    this.form.reset({ warehouseId: 1, date: new Date().toISOString().substring(0, 10), reference: '', supplier: '', notes: '' });
    while (this.lines.length) { this.lines.removeAt(0); }
  }
}
