import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../core/services/user.service';
import { InventoryService } from '../../core/services/inventory.service';
import { ToastService } from '../../core/services/toast.service';
import { Role, User } from '../../core/models/inventory.model';

@Component({
  selector: 'app-users',
  imports: [ReactiveFormsModule],
  templateUrl: './users.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  private readonly usersService = inject(UserService);
  private readonly inventory = inject(InventoryService);
  protected readonly toast = inject(ToastService);
  readonly users = this.usersService.users;
  readonly warehouses = this.inventory.warehouses;
  readonly search = signal('');
  readonly roleFilter = signal('Semua Peran');
  readonly showForm = signal(false);

  readonly form = new FormGroup({
    fullName: new FormControl('', [Validators.required]),
    username: new FormControl('', [Validators.required]),
    password: new FormControl(''),
    role: new FormControl<Role>('Staf Gudang', [Validators.required]),
    warehouseIds: new FormControl<number[]>([], [Validators.required]),
  });

  readonly visible = computed(() => {
    const q = this.search().trim().toLowerCase();
    const r = this.roleFilter();
    return this.users().filter(u => {
      const matchSearch =
        !q ||
        u.fullName.toLowerCase().includes(q) ||
        u.username.toLowerCase().includes(q);
      const matchRole = r === 'Semua Peran' || u.role === r;
      return matchSearch && matchRole;
    });
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.usersService.add(
      {
        username: v.username ?? '',
        fullName: v.fullName ?? '',
        role: (v.role as Role) || 'Staf Gudang',
        isActive: true,
        warehouseIds: v.warehouseIds ?? [],
        lastLoginAt: 'Belum pernah',
      },
      v.password || `${v.username}123`,
    );
    this.toast.show(`Pengguna ${v.fullName} berhasil ditambahkan`, 'success');
    this.showForm.set(false);
    this.form.reset({ role: 'Staf Gudang', warehouseIds: [] });
  }

  toggleWarehouse(id: number): void {
    const current = this.form.get('warehouseIds')?.value ?? [];
    const next = current.includes(id) ? current.filter(x => x !== id) : [...current, id];
    this.form.get('warehouseIds')?.setValue(next);
  }

  warehouseNames(ids: number[]): string {
    if (!ids || !ids.length) return '—';
    return this.warehouses()
      .filter(w => ids.includes(w.id))
      .map(w => w.name)
      .join(', ');
  }

  resetPassword(user: User): void {
    const newPass = prompt(`Masukkan password baru untuk ${user.fullName} (${user.username}):`, 'pass123');
    if (!newPass || !newPass.trim()) return;
    this.usersService.resetPassword(user.id, newPass.trim());
    this.toast.show(`Password ${user.fullName} berhasil direset`, 'success');
  }
}
