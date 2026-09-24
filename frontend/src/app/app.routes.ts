import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: '', canActivate: [authGuard], loadComponent: () => import('./layout/shell.component').then(m => m.ShellComponent), children: [
    { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent), data: { title: 'Dashboard' } },
    { path: 'barang', loadComponent: () => import('./features/master/items.component').then(m => m.ItemsComponent), canActivate: [adminGuard], data: { title: 'Data Barang' } },
    { path: 'gudang', loadComponent: () => import('./features/master/warehouses.component').then(m => m.WarehousesComponent), canActivate: [adminGuard], data: { title: 'Data Gudang' } },
    { path: 'barang-masuk', loadComponent: () => import('./features/transactions/inbound.component').then(m => m.InboundComponent), data: { title: 'Barang Masuk' } },
    { path: 'barang-keluar', loadComponent: () => import('./features/transactions/outbound.component').then(m => m.OutboundComponent), data: { title: 'Barang Keluar' } },
    { path: 'riwayat', loadComponent: () => import('./features/transactions/history.component').then(m => m.HistoryComponent), data: { title: 'Riwayat Transaksi' } },
    { path: 'kartu-stok', loadComponent: () => import('./features/transactions/stock-card.component').then(m => m.StockCardComponent), data: { title: 'Kartu Stok' } },
    { path: 'notifikasi', loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent), data: { title: 'Notifikasi' } },
    { path: 'laporan', loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent), data: { title: 'Laporan' } },
    { path: 'pengguna', loadComponent: () => import('./features/admin/users.component').then(m => m.UsersComponent), canActivate: [adminGuard], data: { title: 'Manajemen Pengguna' } },
    { path: 'audit-log', loadComponent: () => import('./features/admin/audit-log.component').then(m => m.AuditLogComponent), canActivate: [adminGuard], data: { title: 'Audit Log' } },
  ]},
  { path: '**', redirectTo: 'dashboard' },
];
