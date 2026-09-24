import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { TransactionService } from '../../core/services/transaction.service';
import { ToastService } from '../../core/services/toast.service';
import { StockTransaction, TransactionLine, TransactionType } from '../../core/models/inventory.model';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistoryComponent {
  private readonly transactions = inject(TransactionService);
  protected readonly toast = inject(ToastService);
  readonly search = signal('');
  readonly type = signal('Semua');
  readonly all = this.transactions.transactions;

  readonly visible = computed(() => {
    const q = this.search().trim().toLowerCase();
    return this.all().filter(tx => {
      const matchesType = this.type() === 'Semua' || tx.type === this.type();
      const matchesSearch =
        !q ||
        tx.transactionNo.toLowerCase().includes(q) ||
        tx.lines.some(l => l.itemName.toLowerCase().includes(q));
      return matchesType && matchesSearch;
    });
  });

  typeLabel(type: TransactionType): string {
    return type === 'IN' ? 'Masuk' : type === 'OUT' ? 'Keluar' : 'Koreksi';
  }

  totalQty(lines: TransactionLine[]): number {
    return lines.reduce((sum, line) => sum + line.quantity, 0);
  }

  cancel(tx: StockTransaction): void {
    const reason = prompt(`Masukkan alasan pembatalan transaksi ${tx.transactionNo}:`);
    if (!reason || !reason.trim()) return;

    this.transactions.cancelTransaction(tx.id, reason.trim());
    this.toast.show(`Transaksi ${tx.transactionNo} berhasil dibatalkan (koreksi dibuat)`, 'success');
  }

  exportTransactions(): void {
    window.open('/api/reports/export?type=transactions', '_blank');
    this.toast.show('Mengunduh laporan transaksi CSV...', 'success');
  }
}
