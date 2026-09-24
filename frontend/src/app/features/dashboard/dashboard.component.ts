import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AlertService } from '../../core/services/alert.service';
import { TransactionService } from '../../core/services/transaction.service';
import { AuthService } from '../../core/services/auth.service';
import { InventoryService } from '../../core/services/inventory.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly dashboard = inject(DashboardService);
  private readonly alerts = inject(AlertService);
  private readonly transactions = inject(TransactionService);
  private readonly auth = inject(AuthService);
  private readonly inventory = inject(InventoryService);

  readonly user = this.auth.currentUser;
  readonly warehouses = this.inventory.warehouses;
  readonly summary = this.dashboard.summary;
  readonly categoryStock = this.dashboard.categoryStock;
  readonly trend = this.dashboard.trend;
  readonly criticalAlerts = computed(() => this.alerts.alerts().filter(a => !a.isRead).slice(0, 4));
  readonly recentTransactions = computed(() => this.transactions.transactions().slice(0, 3));

  readonly totalCategoryUnits = computed(() =>
    this.categoryStock().reduce((acc, cat) => acc + cat.value, 0),
  );

  readonly today = new Date()
    .toLocaleDateString('id-ID', {
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    })
    .toUpperCase();

  onWarehouseFilter(val: string): void {
    const id = val === 'all' ? undefined : Number(val);
    this.dashboard.loadDashboard(id);
  }

  formatNumber(value: number): string {
    return value.toLocaleString('id-ID');
  }

  formatCurrency(value: string): string {
    return value;
  }
}
