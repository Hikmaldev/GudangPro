# GudangPro Backend — Sistem Manajemen Inventori Gudang

Backend REST API untuk sistem manajemen inventori gudang multi-gudang bagi UKM manufaktur, dibangun sesuai spesifikasi [PRD-Sistem-Manajemen-Inventori-Gudang.md](../../PRD-Sistem-Manajemen-Inventori-Gudang.md) menggunakan **C# / .NET 10 Web API**, **Entity Framework Core**, dan **SQL Server LocalDB**.

---

## 🏛️ Arsitektur Berlapis (Clean Layered Architecture)

Sesuai PRD §9.7:
```
backend/
├── src/
│   ├── GudangPro.Domain/             # Entity Models, Enums (Aturan Domain murni)
│   │   ├── Entities/                 # User, Warehouse, Item, StockLevel, StockTransaction, Alert, AuditLog
│   │   └── Enums/                    # TransactionType (IN/OUT/ADJUST), UserRole
│   │
│   ├── GudangPro.Infrastructure/     # EF Core DbContext, Fluent API, Migrasi, Seeder
│   │   └── Data/
│   │       ├── AppDbContext.cs       # DbContext dengan rowversion concurrency token
│   │       └── DbInitializer.cs      # Seeder pengguna awal, master data & saldo stok
│   │
│   ├── GudangPro.Application/        # Logika Bisnis, Services, DTOs
│   │   ├── DTOs/                     # Auth, Master, Transaksi, Stok, Dashboard DTOs
│   │   └── Services/                 # AuthService, TransactionService, InventoryService,
│   │                                 # StockService, DashboardService, AlertService, AuditService
│   │
│   └── GudangPro.Api/                # Controllers REST API, Program.cs, Konfigurasi
│       ├── Controllers/              # 11 Controller (26 API routes)
│       ├── Program.cs                # Serilog, EF Core, JWT Bearer, CORS, Swagger
│       └── appsettings.json          # Connection string SQL Server & JWT Config
│
└── tests/
    └── GudangPro.Tests/              # xUnit Unit & Integration Tests untuk logika stok
        └── StockBusinessLogicTests.cs # 8 skenario pengujian bisnis stok
```

---

## ⚡ Prasyarat & Menjalankan

### Prasyarat
- **.NET SDK 10.0+** (terinstal)
- **SQL Server LocalDB 2022** (instance `MSSQLLocalDB` aktif)

### Menjalankan Backend
Dari folder root:
```bash
npm run backend
# atau
cd backend/src/GudangPro.Api
dotnet run
```
API akan aktif di:
- **API Base URL**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/` (dokumentasi interaktif & uji coba endpoint)

### Menjalankan Unit Test
```bash
npm run backend:test
# atau
dotnet test backend
```

---

## 🔑 Akun Bawaan (Seeder)

Database diinisialisasi otomatis dengan 3 peran pengguna:

| Username | Password | Nama Lengkap | Peran (Role) | Gudang Akses |
|---|---|---|---|---|
| `admin` | `admin123` | Budi Santoso | Admin Gudang | Semua Gudang |
| `sari` | `sari123` | Sari Wulandari | Staf Gudang | Gudang Utama, Gudang Bahan Baku |
| `hendra` | `hendra123` | Hendra Wijaya | Pemilik | Gudang Utama, Gudang Barang Jadi |

---

## 📋 Daftar Endpoint REST API (PRD §10.4)

Semua endpoint kecuali `/api/auth/*` diproteksi menggunakan **JWT Bearer Token** dan **Role-Based Access Control (RBAC)**.

### Autentikasi (`/api/auth`)
- `POST /api/auth/login` — Login username/password, respon access token + refresh token + data pengguna
- `POST /api/auth/refresh` — Refresh access token sebelum kedaluwarsa

### Manajemen Barang (`/api/items`)
- `GET /api/items` — Daftar barang (bisa filter `search` & `status`)
- `GET /api/items/{id}` — Detail barang
- `POST /api/items` — Tambah barang baru (Admin Gudang, validasi kode unik FR-MST-02)
- `PUT /api/items/{id}` — Edit data barang & batas stok minimum (Admin Gudang)
- `PATCH /api/items/{id}/deactivate` — Nonaktifkan barang tanpa menghapus transaksi (FR-MST-05)

### Manajemen Gudang (`/api/warehouses`)
- `GET /api/warehouses` — Daftar gudang beserta ringkasan stok
- `POST /api/warehouses` — Tambah gudang baru (Admin Gudang)
- `PUT /api/warehouses/{id}` — Edit gudang (Admin Gudang)

### Master Kategori & Satuan
- `GET /api/categories` — Daftar kategori barang
- `GET /api/units` — Daftar satuan barang (Pcs, Kg, Roll, Box, Liter)

### Transaksi Stok (`/api/transactions`)
- `POST /api/transactions/in` — Catat barang masuk (tambah stok gudang, tolak qty <= 0)
- `POST /api/transactions/out` — Catat barang keluar (kurangi stok, tolak jika qty > stok, otomatis buat alert jika stok <= minStock)
- `POST /api/transactions/{id}/cancel` — Batalkan transaksi dengan membuat **transaksi koreksi** (tipe `ADJUST`, Aturan Bisnis #3)
- `GET /api/transactions` — Riwayat transaksi dengan filter (gudang, tanggal, tipe, barang, pencarian) dan pagination
- `GET /api/transactions/{id}` — Detail transaksi lengkap dengan baris barang

### Informasi Stok & Kartu Stok (`/api/stock`)
- `GET /api/stock` — Monitoring saldo stok per barang per gudang dengan status Aman/Menipis/Kritis
- `GET /api/stock/{itemId}/card` — **Kartu Stok (FR-HIS-05)**: saldo awal, riwayat pergerakan (masuk/keluar/saldo berjalan), saldo akhir

### Dashboard & Analitik (`/api/dashboard`)
- `GET /api/dashboard/summary` — Ringkasan metrik (total barang, transaksi hari ini, barang kritis, estimasi nilai)
- `GET /api/dashboard/charts` — Distribusi stok per kategori (diagram lingkaran) & tren masuk/keluar 7 hari (diagram batang)

### Peringatan Stok (`/api/alerts`)
- `GET /api/alerts` — Daftar alert stok minimum (filter kritis/menipis/belum dibaca)
- `PATCH /api/alerts/{id}/read` — Tandai notifikasi telah dibaca
- `POST /api/alerts/mark-all-read` — Tandai semua notifikasi telah dibaca

### Manajemen Pengguna (`/api/users`) — *Admin Gudang*
- `GET /api/users` — Daftar pengguna sistem
- `POST /api/users` — Tambah akun pengguna baru dan penugasan gudang
- `PUT /api/users/{id}` — Edit data pengguna & gudang yang ditugaskan
- `POST /api/users/{id}/reset-password` — Reset password pengguna (FR-AUTH-05)

### Jejak Audit (`/api/audit-logs`) — *Admin Gudang*
- `GET /api/audit-logs` — Jejak audit aktivitas perubahan data (siapa, kapan, aksi apa, entitas mana, IP address)

### Laporan (`/api/reports`) — *Admin Gudang & Pemilik*
- `GET /api/reports/export?type=stock` — Unduh laporan stok CSV kompatibel Excel
- `GET /api/reports/export?type=transactions` — Unduh laporan transaksi CSV kompatibel Excel

---

## 🛡️ Aturan Bisnis yang Diimplementasikan

1. **Perubahan stok hanya melalui transaksi**: Tidak ada operasi update langsung ke saldo stok; semua penambahan atau pengurangan terjadi melalui transaksi masuk, transaksi keluar, atau transaksi koreksi.
2. **Stok tidak boleh bernilai negatif**: Transaksi barang keluar divalidasi ketat terhadap saldo tersedia; jika diminta melebihi stok, transaksi langsung ditolak.
3. **Optimistic Concurrency**: Kolom `RowVersion` (`[Timestamp]`) pada entitas `StockLevel` mendeteksi dan mencegah tabrakan pembaruan data bersamaan oleh staf berbeda.
4. **Database Transaction**: Operasi multi-baris pada transaksi dibungkus dalam `BeginTransactionAsync()`; jika satu baris gagal, seluruh perubahan di-rollback.
5. **Pembatalan Transaksi via Koreksi**: Transaksi yang sudah tersimpan tidak pernah dihapus fisik dari database; pembatalan membuat transaksi koreksi bertipe `ADJUST` dan membalik saldo stok.
6. **Alert Otomatis**: Saat stok tersisa `<= MinStock`, sistem otomatis menerbitkan notifikasi bertingkat (`menipis` jika `<= MinStock`, `kritis` jika `<= MinStock / 2`).
7. **Keamanan Akun (FR-AUTH-04)**: Akun terkunci otomatis selama 15 menit jika terjadi 5 kali percobaan login dengan password salah secara berurutan.
8. **Soft-Delete Barang (FR-MST-05)**: Barang yang dinonaktifkan tidak dihapus fisik, menjaga integritas histori transaksi.
