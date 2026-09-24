import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AlertService } from '../../core/services/alert.service';
import { ToastService } from '../../core/services/toast.service';
import { AlertLevel } from '../../core/models/inventory.model';

@Component({
  selector: 'app-alerts',
  templateUrl: './alerts.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertsComponent {
  private readonly alertsService = inject(AlertService);
  protected readonly toast = inject(ToastService);
  readonly alerts = this.alertsService.alerts;
  readonly filter = signal('Semua');

  readonly visible = computed(() => {
    if (this.filter() === 'Semua') return this.alerts();
    if (this.filter() === 'Belum dibaca') return this.alerts().filter(a => !a.isRead);
    return this.alerts().filter(a => !a.isRead && a.level === this.filter().toLowerCase());
  });

  readonly unreadCount = computed(() => this.alerts().filter(a => !a.isRead).length);
  readonly criticalCount = computed(() => this.alerts().filter(a => a.level === 'kritis').length);
  readonly warningCount = computed(() => this.alerts().filter(a => a.level === 'menipis').length);

  markAllRead(): void {
    this.alertsService.markAllRead();
    this.toast.show('Semua notifikasi ditandai sudah dibaca', 'success');
  }

  markRead(level: AlertLevel, id: number): void {
    this.alertsService.markRead(id);
    this.toast.show(`Notifikasi ${level} ditandai dibaca`, 'success');
  }

  levelIcon(level: AlertLevel): string {
    return level === 'kritis' ? '!' : '!';
  }
}