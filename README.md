# 📦 GudangPro — Sistem Manajemen Inventori Gudang (WMS)

> Sistem Manajemen Inventori Gudang berbasis web untuk UKM Manufaktur. Dirancang dengan **Clean Layered Architecture** pada backend (.NET 10 Web API) dan **Modern Angular 20 Standalone** pada frontend.

---

## 🚀 Ringkasan Teknologi

- **Backend:** C# / .NET 10 Web API, Entity Framework Core, SQL Server
- **Frontend:** Angular 20, Standalone Components, Angular Signals, Reactive Forms, OnPush Change Detection
- **Keamanan:** JWT Bearer Token, Refresh Token, BCrypt Password Hashing, Role-Based Access Control (RBAC), Account Lockout
- **Integritas Data:** Optimistic Concurrency Control (`RowVersion`), Pencegahan Stok Negatif, Transaksi Koreksi (`ADJUST`)
- **Logging & Dokumentasi:** Serilog Request Logging, Swagger UI (OpenAPI)
- **Containerization:** Docker & Docker Compose (Multi-stage build)

---

## 📂 Struktur Repositori

```text
inventori-gudang/
├── backend/                      # Backend .NET 10 Solution
│   ├── src/
│   │   ├── GudangPro.Domain/         # Entity, Enums, Aturan Bisnis
│   │   ├── GudangPro.Infrastructure/ # EF Core DbContext, Migrations, Seed Data
│   │   ├── GudangPro.Application/    # Application Services & DTOs
│   │   └── GudangPro.Api/            # REST Controllers, JWT, Swagger, Serilog
│   ├── tests/
│   │   └── GudangPro.Tests/          # xUnit Unit & Integration Tests
│   ├── Dockerfile
│   └── README.md
│
├── frontend/                     # Frontend Angular 20
│   ├── src/
│   │   └── app/
│   │       ├── core/                 # Services, Interceptors, Guards, Models
│   │       ├── features/             # Halaman: Dashboard, Master, Transaksi, Laporan, Pengguna, Audit
│   │       └── layout/               # Shell layout & navigasi
│   ├── Dockerfile
│   ├── nginx.conf
│   └── README.md
│
├── docker-compose.yml            # Multi-container setup (SQL Server, Backend, Frontend)
├── package.json                  # Root runner scripts
├── SETUP-GUIDE.md                # Panduan instalasi dan database lokal
└── README.md
```

---

## 🛠️ Cara Menjalankan Project

### Opsi A: Development Lokal (Native)

#### 1. Prasyarat
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+ & npm](https://nodejs.org)
- Microsoft SQL Server (Express / LocalDB)

#### 2. Jalankan Backend Web API
```bash
npm run backend
```
- API berjalan di: `http://localhost:5000`
- Swagger UI dokumentasi interaktif: `http://localhost:5000/`

#### 3. Jalankan Frontend Angular
```bash
npm run dev
```
- Frontend berjalan di: `http://localhost:4200` (dengan reverse proxy otomatis ke `/api/*`)

#### 4. Jalankan Unit Tests Backend
```bash
npm run backend:test
```

---

### Opsi B: Menggunakan Docker Compose

Jika memiliki Docker Desktop:
```bash
# Menjalankan database, backend, dan frontend sekaligus
npm run docker:up

# Mematikan container
npm run docker:down
```

---

## 👤 Akun Bawaan untuk Pengujian (Seed Users)

| Username | Password | Peran | Deskripsi Akses |
|---|---|---|---|
| `admin` | `admin123` | **Admin Gudang** | Akses penuh seluruh modul termasuk Master Data, Manajemen Pengguna, dan Audit Log |
| `sari` | `sari123` | **Staf Gudang** | Operasional Barang Masuk, Barang Keluar, Riwayat Transaksi, Kartu Stok, dan Notifikasi |
| `hendra` | `hendra123` | **Pemilik** | Dashboard Eksekutif, Monitoring Transaksi, dan Ekspor Laporan |

---

## 📋 Fitur Utama Sistem

1. **Dashboard Eksekutif:** Ringkasan KPI stok, grafik distribusi kategori, tren 7 hari transaksi, peringatan stok kritis.
2. **Master Data Barang & Gudang:** Validasi kode unik, batas stok minimum, dan soft-delete (penonaktifan).
3. **Pencatatan Transaksi:** Validasi stok masuk & keluar secara real-time dengan pencegahan saldo negatif.
4. **Riwayat & Pembatalan Transaksi:** Pembatalan menggunakan transaksi pembalik (`ADJUST`) demi jejak audit yang akurat.
5. **Kartu Stok:** Riwayat mutasi kronologis dan perhitungan saldo berjalan per barang per gudang.
6. **Alert Notifikasi Otomatis:** Deteksi stok menipis dan kritis ketika saldo $\le$ batas minimum.
7. **Laporan & Ekspor:** Unduh laporan inventori dan mutasi dalam format CSV.
8. **Audit Trail:** Pencatatan otomatis setiap aksi pengguna, alamat IP, dan waktu eksekusi.
9. **Keamanan Akun:** Penguncian otomatis 15 menit setelah 5 kali gagal login berturut-turut.

---

## 📄 Lisensi

Proyek ini dibuat untuk keperluan internal manajemen inventori gudang UKM Manufaktur.
