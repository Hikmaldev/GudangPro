import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { AlertService } from '../core/services/alert.service';
import { NavItem } from '../shared/models/inventory.models';

const TITLES: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/barang': 'Data Barang',
  '/gudang': 'Data Gudang',
  '/barang-masuk': 'Barang Masuk',
  '/barang-keluar': 'Barang Keluar',
  '/riwayat': 'Riwayat Transaksi',
  '/kartu-stok': 'Kartu Stok',
  '/notifikasi': 'Notifikasi',
  '/laporan': 'Laporan',
  '/pengguna': 'Manajemen Pengguna',
  '/audit-log': 'Audit Log',
};

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private readonly alertService = inject(AlertService);

  readonly menuOpen = signal(false);
  readonly user = this.auth.currentUser;
  readonly unreadAlertsCount = computed(() => this.alertService.alerts().filter(a => !a.isRead).length);

  readonly primary: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: '▦' },
    { label: 'Data Barang', route: '/barang', icon: '▧', adminOnly: true },
    { label: 'Data Gudang', route: '/gudang', icon: '⌂', adminOnly: true },
    { label: 'Barang Masuk', route: '/barang-masuk', icon: '↓' },
    { label: 'Barang Keluar', route: '/barang-keluar', icon: '↑' },
    { label: 'Riwayat Transaksi', route: '/riwayat', icon: '↻' },
  ];

  readonly secondary = computed<NavItem[]>(() => [
    { label: 'Kartu Stok', route: '/kartu-stok', icon: '▤' },
    { label: 'Notifikasi', route: '/notifikasi', icon: '♢', badge: this.unreadAlertsCount() },
    { label: 'Laporan', route: '/laporan', icon: '▤' },
    { label: 'Manajemen Pengguna', route: '/pengguna', icon: '♙', adminOnly: true },
    { label: 'Audit Log', route: '/audit-log', icon: '◉', adminOnly: true },
  ]);

  pageTitle(): string {
    return TITLES[this.router.url.split('?')[0]] ?? 'Dashboard';
  }

  initials(): string {
    const name = this.user()?.fullName ?? 'Budi Santoso';
    return name
      .split(' ')
      .map(part => part[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  visible(items: NavItem[]): NavItem[] {
    return items.filter(item => !item.adminOnly || this.user()?.role === 'Admin Gudang');
  }

  toggleMenu(): void { this.menuOpen.update(open => !open); }
  closeMenu(): void { this.menuOpen.set(false); }
  logout(): void { this.auth.logout(); }
}