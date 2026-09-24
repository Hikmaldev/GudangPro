import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AuditService } from '../../core/services/audit.service';

@Component({
  selector: 'app-audit-log',
  templateUrl: './audit-log.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditLogComponent {
  private readonly audit = inject(AuditService);
  readonly logs = this.audit.logs;
  readonly search = signal('');
  readonly action = signal('Semua');

  readonly visible = computed(() => {
    const q = this.search().trim().toLowerCase();
    return this.logs().filter(log => {
      const matchesAction = this.action() === 'Semua' || log.action === this.action();
      const matchesSearch = !q || log.entityName.toLowerCase().includes(q) || log.detail.toLowerCase().includes(q) || log.userFullName.toLowerCase().includes(q);
      return matchesAction && matchesSearch;
    });
  });
}