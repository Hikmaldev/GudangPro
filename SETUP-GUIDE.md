# 📦 GudangPro — Panduan Setup, Database, API & Docker

Dokumen ini berisi panduan lengkap untuk menjalankan aplikasi **GudangPro (Sistem Manajemen Inventori Gudang)**, cara membuka dan mengelola database, status implementasi API layer & autentikasi, serta petunjuk penggunaan Docker.

---

## 📑 Daftar Isi
1. [Cara Cepat Menjalankan Aplikasi](#1-cara-cepat-menjalankan-aplikasi)
2. [Cara Mengakses & Membuka Database](#2-cara-mengakses--membuka-database)
3. [Status Penerapan API Layer & Autentikasi](#3-status-penerapan-api-layer--autentikasi)
4. [Apakah Perlu Docker? & Cara Menggunakannya](#4-apakah-perlu-docker--cara-menggunakannya)
5. [Daftar Akun Pengujian (Seed Accounts)](#5-daftar-akun-pengujian-seed-accounts)
6. [Struktur Endpoint REST API](#6-struktur-endpoint-rest-api)

---

## 1. Cara Cepat Menjalankan Aplikasi

Aplikasi terdiri dari 2 bagian: **Backend (.NET 10 Web API)** dan **Frontend (Angular 20 Standalone)**.

### Langkah 1: Jalankan Backend Web API
Buka Terminal / PowerShell di root folder project:
```bash
npm run backend
```
*Backend akan berjalan di:* `http://localhost:5000`  
*Swagger UI Dokumentasi API:* `http://localhost:5000/`

> **Catatan:** Saat backend pertama kali dijalankan, Entity Framework Core secara otomatis mengecek database `GudangProDb`, membuat semua tabel (bila belum ada), dan mengisinya dengan data awal (seed data master barang, gudang, user, stok, dan kategori).

### Langkah 2: Jalankan Frontend Angular
Buka Terminal / PowerShell baru:
```bash
npm run dev
```
*Frontend akan berjalan di:* `http://localhost:4200`

> Frontend sudah dilengkapi reverse proxy (`proxy.conf.json`) sehingga request `/api/*` langsung diteruskan ke backend `http://localhost:5000`.

---

## 2. Cara Mengakses & Membuka Database

Database yang digunakan saat ini adalah **Microsoft SQL Server Express** (instance `SQLEXPRESS`), dengan nama database **`GudangProDb`**.

### Lokasi File Fisik Database
- **File Data (.mdf):** `C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\GudangProDb.mdf`
- **File Log (.ldf):** `C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\GudangProDb_log.ldf`

### Parameter Koneksi Database
| Parameter | Nilai |
|---|---|
| **DBMS** | Microsoft SQL Server (SQLEXPRESS) |
| **Server Name / Host** | `.\SQLEXPRESS` atau `localhost\SQLEXPRESS` |
| **Database Name** | `GudangProDb` |
| **Authentication** | **Windows Authentication** (Integrated Security) |
| **Trust Server Certificate** | `True` |
| **Connection String** | `Server=.\SQLEXPRESS;Database=GudangProDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;` |

---

### Pilihan Tools untuk Membuka Database

#### Opsi A: DBeaver (Sudah Terhubung)
1. Buka DBeaver.
2. Pada koneksi MS SQL Server:
   - **Host:** `localhost\SQLEXPRESS` atau `.\SQLEXPRESS`
   - **Database:** `GudangProDb`
   - **Authentication:** `Windows Authentication`
   - Centang **Trust Server Certificate**.
3. Klik **Connect**. Seluruh tabel (`Items`, `StockLevels`, `StockTransactions`, dll.) siap diakses.

---

#### Opsi B: Azure Data Studio / VS Code
1. Masukkan **Server name:** `.\SQLEXPRESS`
2. **Database name:** `GudangProDb`
3. **Authentication:** `Windows Authentication` (Integrated)
4. **Trust server certificate:** `True`
1. Buka tab Extensions (`Ctrl+Shift+X`), cari dan install: **SQL Server (mssql)** buatan Microsoft.
2. Klik ikon database (MSSQL) di sidebar kiri.
3. Klik tanda `+` (Add Connection):
   - Server name: `(localdb)\MSSQLLocalDB`
   - Database name: `GudangProDb`
   - Authentication Type: `Integrated`
4. Tekan Enter dan beri nama koneksi (misal: `GudangPro Local`).
5. Anda bisa langsung menjalankan query SQL langsung di dalam editor VS Code.

---

#### Opsi D: SQL Server Management Studio (SSMS)
1. Buka SSMS.
2. Di jendela *Connect to Server*:
   - **Server type:** `Database Engine`
   - **Server name:** `(localdb)\MSSQLLocalDB`
   - **Authentication:** `Windows Authentication`
3. Klik **Connect**.
4. Di *Object Explorer*, expand **Databases** > **GudangProDb** > **Tables**.

---

#### Opsi E: Lewat PowerShell (Tanpa Perlu Install Software Tambahan)
Jika Anda hanya ingin mengecek isi data secara cepat tanpa install software GUI:
```powershell
# Cek daftar tabel
powershell -Command "$c = New-Object System.Data.SqlClient.SqlConnection('Server=(localdb)\MSSQLLocalDB;Database=GudangProDb;Trusted_Connection=True;TrustServerCertificate=True;'); $c.Open(); $cmd = $c.CreateCommand(); $cmd.CommandText = 'SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = ''BASE TABLE'''; $r = $cmd.ExecuteReader(); while($r.Read()) { Write-Output $r[0] }; $c.Close();"

# Cek daftar user
powershell -Command "$c = New-Object System.Data.SqlClient.SqlConnection('Server=(localdb)\MSSQLLocalDB;Database=GudangProDb;Trusted_Connection=True;TrustServerCertificate=True;'); $c.Open(); $cmd = $c.CreateCommand(); $cmd.CommandText = 'SELECT Id, Username, FullName, Role, IsActive FROM Users'; $r = $cmd.ExecuteReader(); while($r.Read()) { Write-Output ($r[0].ToString() + ' | ' + $r[1] + ' | ' + $r[2] + ' | ' + $r[3]) }; $c.Close();"
```

---

### Struktur Tabel di Database `GudangProDb`

Database ini dirancang sesuai spesifikasi PRD bagian 10.3 (Data Integrity & Concurrency):

1. **`Users`**: Akun pengguna, peran (`Role`), status aktif, password hash, failed login count, dan lockout timestamp.
2. **`Warehouses`**: Data master gudang (kode, nama, alamat, status).
3. **`Categories`**: Kategori barang (Bahan Baku, Barang Jadi, Kemasan, Suku Cadang).
4. **`Units`**: Satuan ukuran barang (Kg, Pcs, Roll, Box, Liter).
5. **`Items`**: Master data barang, relasi kategori, satuan, dan stok minimum (`MinStock`).
6. **`StockLevels`**: Stok per gudang (`ItemId`, `WarehouseId`, `Quantity`, dan kolom `RowVersion` tipe `timestamp` untuk **optimistic concurrency control**).
7. **`StockTransactions`**: Header mutasi inventori (`IN`, `OUT`, `ADJUST`), nomor transaksi unik, status (`Berhasil`/`Dibatalkan`), nomor referensi, dan user pembuat.
8. **`StockTransactionLines`**: Rincian baris barang dan jumlah per transaksi.
9. **`UserWarehouses`**: Relasi penugasan user ke satu atau lebih gudang.
10. **`Alerts`**: Peringatan stok menipis/kritis otomatis saat stok $\le$ stok minimum.
11. **`AuditLogs`**: Jejak audit setiap perubahan data (siapa, aksi apa, entitas mana, alamat IP, waktu).

---

## 3. Status Penerapan API Layer & Autentikasi

### Apakah API Layer Sudah Diterapkan?
**SUDAH 100% LENGKAP.**
Backend dibangun menggunakan arsitektur **Clean Architecture** dengan 4 layer terpisah di folder `backend/src/`:
- **`GudangPro.Domain`**: Entity domain, enum transaksi, dan aturan bisnis.
- **`GudangPro.Infrastructure`**: EF Core `AppDbContext`, mapping optimistic concurrency `[Timestamp]`, dan seed data otomatis (`DbInitializer`).
- **`GudangPro.Application`**: Business logic services: `AuthService`, `InventoryService`, `TransactionService`, `StockService`, `DashboardService`, `AlertService`, dan `AuditService`.
- **`GudangPro.Api`**: Layer presentasi REST API dengan 11 Controller, Serilog request logging, Swagger OpenAPI, dan CORS middleware.

### Apakah Autentikasi & Otorisasi Sudah Diterapkan?
**SUDAH 100% LENGKAP.**
Fitur keamanan yang diterapkan mencakup:
1. **JWT Bearer Token**:
   - `POST /api/auth/login`: Menghasilkan `accessToken` (berlaku 1 jam) dan `refreshToken` (berlaku 7 hari).
   - `POST /api/auth/refresh`: Memperbarui token tanpa mengharuskan pengguna login ulang.
2. **Keamanan Password**:
   - Password di-hash menggunakan **BCrypt** (`BCrypt.Net-Next`) dengan salt kuat. Tidak ada plain text password di database.
3. **Proteksi Brute-force & Account Lockout (FR-AUTH-04)**:
   - Jika pengguna salah memasukkan password sebanyak 5 kali berturut-turut, akun otomatis terkunci (**locked out**) selama 15 menit.
   - Tercatat langsung ke tabel `AuditLogs` dengan aksi `KunciAkun`.
4. **Role-Based Access Control (RBAC)**:
   - 3 Peran: `Admin Gudang`, `Staf Gudang`, `Pemilik`.
   - Endpoint sensitif dilindungi atribut `[Authorize(Roles = "Admin Gudang")]` (seperti manajemen pengguna, penonaktifan barang, dan audit log).
5. **Frontend HTTP Interceptor**:
   - File `frontend/src/app/core/interceptors/auth.interceptor.ts` secara otomatis menyisipkan header `Authorization: Bearer <token>` pada setiap pemanggilan API.

---

## 4. Apakah Perlu Docker? & Cara Menggunakannya

### Analisis: Apakah Proyek Ini Perlu Docker?

| Skenario | Rekomendasi | Alasan |
|---|---|---|
| **Development di Komputer Anda Saat Ini (Windows)** | **Opsional (Native sudah sangat baik)** | .NET 10 SDK, Node.js, dan SQL Server LocalDB sudah terpasang. Menjalankannya secara native (`npm run backend` dan `npm run dev`) memberikan kecepatan startup dan debugging seketika tanpa beban resource container. |
| **Kolaborasi Tim (Ada pengguna Mac / Linux)** | **SANGAT DISARANKAN** | SQL Server LocalDB hanya bekerja di Windows. Di Mac atau Linux, tim wajib menggunakan Docker untuk menjalankan container SQL Server. |
| **Deployment ke Server / VPS (Ubuntu, Debian, Cloud)** | **WAJIB / BEST PRACTICE** | Docker memastikan backend .NET, SQL Server Linux, dan frontend Nginx berjalan identik di server mana pun dengan satu kali perintah. |

### Penerapan Docker yang Sudah Disediakan
Konfigurasi Docker lengkap sudah dibuat di project ini:
- `backend/Dockerfile`: Multi-stage build .NET 10 SDK & ASP.NET Core Runtime.
- `frontend/Dockerfile`: Multi-stage build Node 22 & Nginx Alpine.
- `frontend/nginx.conf`: Nginx web server dengan reverse proxy `/api/` dan routing SPA.
- `docker-compose.yml`: Orkestrasi 3 service:
  1. `sqlserver`: SQL Server 2022 Express Linux container (Port 1433, sa password: `GudangPro@2026!`).
  2. `backend`: ASP.NET Core Web API .NET 10 (Port 5000).
  3. `frontend`: Angular 20 + Nginx (Port 4200).

### Cara Menjalankan dengan Docker (Bila Docker Desktop Sudah Terpasang)
Jika komputer Anda sudah memiliki Docker Desktop:
```bash
# Menjalankan seluruh stack (Database + Backend + Frontend) di background
npm run docker:up

# Melihat log container
npm run docker:logs

# Mematikan container
npm run docker:down
```
Setelah dijalankan dengan Docker:
- Buka Frontend di: `http://localhost:4200`
- Buka Swagger API di: `http://localhost:5000`
- Database SQL Server dapat diakses di: `localhost,1433` (User: `sa`, Password: `GudangPro@2026!`).

---

## 5. Daftar Akun Pengujian (Seed Accounts)

Data akun berikut telah terdaftar otomatis di database:

| Username | Password | Nama Lengkap | Peran | Hak Akses |
|---|---|---|---|---|
| `admin` | `admin123` | Budi Santoso | **Admin Gudang** | Akses penuh ke semua modul, Master Barang, Gudang, Manajemen Pengguna, dan Audit Log. |
| `sari` | `sari123` | Sari Wulandari | **Staf Gudang** | Input Barang Masuk, Barang Keluar, Riwayat Transaksi, Kartu Stok, Notifikasi Alert. |
| `hendra` | `hendra123` | Hendra Wijaya | **Pemilik** | Read-only Dashboard Eksekutif, Riwayat Transaksi, Laporan Inventori & Ekspor CSV. |

---

## 6. Struktur Endpoint REST API

Semua endpoint dapat diuji melalui Swagger UI di `http://localhost:5000/`:

### Autentikasi (`/api/auth`)
- `POST /api/auth/login` — Login username & password, mengembalikan JWT & refresh token.
- `POST /api/auth/refresh` — Memperbarui JWT access token menggunakan refresh token.

### Master Barang (`/api/items`)
- `GET /api/items` — Mendapatkan daftar semua barang dan total stok.
- `GET /api/items/{id}` — Detail barang berdasarkan ID.
- `POST /api/items` — Menambahkan barang baru (*Admin Gudang*).
- `PUT /api/items/{id}` — Memperbarui data barang (*Admin Gudang*).
- `PATCH /api/items/{id}/deactivate` — Nonaktifkan barang (*Admin Gudang*).

### Master Gudang (`/api/warehouses`)
- `GET /api/warehouses` — Mendapatkan daftar gudang dan jumlah jenis barang.
- `POST /api/warehouses` — Menambahkan gudang baru (*Admin Gudang*).
- `PUT /api/warehouses/{id}` — Memperbarui informasi gudang (*Admin Gudang*).

### Kategori & Satuan
- `GET /api/categories` — Daftar kategori barang (Bahan Baku, Barang Jadi, Kemasan, Suku Cadang).
- `GET /api/units` — Daftar satuan ukuran (Pcs, Kg, Roll, Box, Liter).

### Transaksi Inventori (`/api/transactions`)
- `GET /api/transactions` — Riwayat transaksi mutasi stok (paginated, filter tanggal, jenis, gudang).
- `POST /api/transactions/in` — Mencatat transaksi barang masuk (menambah stok).
- `POST /api/transactions/out` — Mencatat transaksi barang keluar (memvalidasi stok tidak boleh negatif).
- `POST /api/transactions/{id}/cancel` — Membatalkan transaksi (membuat transaksi koreksi pembalik `ADJUST`).

### Stok & Kartu Stok (`/api/stock`)
- `GET /api/stock` — Ringkasan stok per barang dan gudang.
- `GET /api/stock/{itemId}/card` — Data kartu stok kronologis per barang dan gudang.

### Dashboard (`/api/dashboard`)
- `GET /api/dashboard/summary` — Statistik KPI (total item, transaksi hari ini, stok menipis, nilai stok).
- `GET /api/dashboard/charts` — Data diagram distribusi kategori dan tren 7 hari transaksi.

### Notifikasi Alert (`/api/alerts`)
- `GET /api/alerts` — Daftar peringatan stok menipis dan kritis.
- `PATCH /api/alerts/{id}/read` — Menandai notifikasi telah dibaca.
- `POST /api/alerts/mark-all-read` — Menandai semua notifikasi telah dibaca.

### Laporan (`/api/reports`)
- `GET /api/reports/export?type=stock` — Ekspor laporan stok ke file CSV.
- `GET /api/reports/export?type=transactions` — Ekspor laporan transaksi ke file CSV.

### Manajemen Pengguna (`/api/users`)
- `GET /api/users` — Daftar pengguna sistem (*Admin Gudang*).
- `POST /api/users` — Menambahkan pengguna baru (*Admin Gudang*).
- `POST /api/users/{id}/reset-password` — Reset password pengguna (*Admin Gudang*).

### Audit Log (`/api/audit-logs`)
- `GET /api/audit-logs` — Jejak audit aktivitas pengguna dan perubahan data (*Admin Gudang*).
