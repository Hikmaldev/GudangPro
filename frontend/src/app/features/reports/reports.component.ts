import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../core/services/toast.service';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportsComponent {
  private readonly http = inject(HttpClient);
  protected readonly toast = inject(ToastService);
  protected readonly dashboard = inject(DashboardService);

  readonly summary = this.dashboard.summary;

  exportStock(): void {
    this.http.get('/api/reports/export?type=stock', { responseType: 'blob' }).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `laporan-stok-${new Date().toISOString().substring(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toast.show('Laporan stok berhasil diunduh (CSV)', 'success');
      },
      error: () => this.toast.show('Gagal mengunduh laporan stok', 'error'),
    });
  }

  exportTransactions(): void {
    this.http.get('/api/reports/export?type=transactions', { responseType: 'blob' }).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `laporan-transaksi-${new Date().toISOString().substring(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toast.show('Laporan transaksi berhasil diunduh (CSV)', 'success');
      },
      error: () => this.toast.show('Gagal mengunduh laporan transaksi', 'error'),
    });
  }
}
