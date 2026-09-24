# 🚀 Panduan Hosting Publik Gratis (Demo GudangPro)

Panduan ini menjelaskan cara mempublikasikan sistem **GudangPro** ke internet **100% GRATIS tanpa biaya sepeser pun**, menggunakan kombinasi teknologi modern:

- **Frontend:** [Vercel](https://vercel.com) (Angular 20 SPA — Sangat cepat, CDN Global, SSL otomatis)
- **Backend:** [Render](https://render.com) (C# / .NET 10 Web API via Docker Container)
- **Database:** [Supabase](https://supabase.com) (Cloud PostgreSQL — Gratis 500MB, auto-schema & auto-seed)

```
[ Browser / Klien ]
        │
        ▼
[ Vercel (Frontend Angular) ]
        │  /api/* (Reverse Proxy via vercel.json)
        ▼
[ Render (Backend .NET 10 API) ]
        │  Connection String (SSL)
        ▼
[ Supabase (PostgreSQL Cloud DB) ]
```

---

## 📑 Daftar Isi
1. [Langkah 1: Setup Database di Supabase](#langkah-1-setup-database-di-supabase)
2. [Langkah 2: Deploy Backend ke Render](#langkah-2-deploy-backend-ke-render)
3. [Langkah 3: Deploy Frontend ke Vercel](#langkah-3-deploy-frontend-ke-vercel)
4. [Langkah 4: Menghubungkan Frontend ke Backend](#langkah-4-menghubungkan-frontend-ke-backend)
5. [Pengujian & Akun Login Demo](#pengujian--akun-login-demo)
6. [Catatan Penting Free Tier](#catatan-penting-free-tier)

---

## Langkah 1: Setup Database di Supabase

Karena Anda sudah memiliki akun Supabase, kita gunakan Supabase sebagai database cloud gratis. Backend GudangPro sudah dilengkapi fitur **otomatis membuat tabel & seeding data** saat pertama kali dijalankan.

1. Buka [database.new](https://database.new) atau login ke dashboard [Supabase](https://supabase.com/dashboard).
2. Klik **New Project**.
   - **Name:** `gudangpro-db` (atau nama lain yang diinginkan)
   - **Database Password:** Buat password yang kuat (dan catat password ini!)
   - **Region:** Pilih region terdekat (misal: `Singapore (ap-southeast-1)`)
3. Tunggu ~1-2 menit hingga status database aktif (*Provisioning* selesai).
4. Ambil **Connection String (Wajib gunakan Connection Pooler / IPv4)**:
   - Klik tombol **Connect** di bagian atas dashboard Supabase (atau Project Settings > Database).
   - Pilih bagian **Connection Pooling** (atau tab **Session Mode**, port 5432).
   - *(PENTING: Jangan gunakan Direct Connection `db.*.supabase.co` karena Supabase direct hanya mendukung IPv6, sedangkan Render menggunakan IPv4).*
   - Contoh format URI (Pooler):
     ```text
     postgresql://postgres.[PROJECT-REF]:[YOUR-PASSWORD]@aws-0-ap-southeast-1.pooler.supabase.com:5432/postgres
     ```
   - Atau contoh format ADO.NET:
     ```text
     Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.[PROJECT-REF];Password=[YOUR-PASSWORD];SSL Mode=Require;
     ```
   *(Ganti `[PROJECT-REF]` dengan ID project Anda dan `[YOUR-PASSWORD]` dengan password database Anda)*.

> 💡 **Info:** Anda tidak perlu menjalankan script SQL manual apa pun! Backend .NET akan otomatis mendeteksi database kosong, membuat seluruh tabel (`Items`, `StockLevels`, `Transactions`, dll.), serta mengisikan data awal (seeder) secara otomatis.

---

## Langkah 2: Deploy Backend ke Render

Backend C# .NET 10 membutuhkan host yang dapat menjalankan Docker. **Render.com** menyediakan *Free Web Service* yang membaca langsung `Dockerfile` dari repositori GitHub Anda.

1. Buka [render.com](https://render.com) dan login (bisa login menggunakan akun GitHub).
2. Klik tombol **New +** di pojok kanan atas > Pilih **Web Service**.
3. Pilih opsi **Build and deploy from a Git repository** > Klik **Next**.
4. Hubungkan dan pilih repositori GitHub Anda: `inventori-gudang`.
5. Isi konfigurasi Web Service:
   - **Name:** `gudangpro-api` (URL API Anda akan menjadi `https://gudangpro-api.onrender.com`)
   - **Region:** `Singapore (Southeast Asia)`
   - **Language:** Pilih **Docker**
   - **Branch:** `main`
   - **Root Directory:** `backend`
   - **Dockerfile Path:** `Dockerfile` (atau kosongkan, Render akan mendeteksi otomatis)
   - **Instance Type:** Pilih **Free** ($0/month)
6. Masukkan **Environment Variables**:
   Gulir ke bagian **Environment Variables**, klik **Add Environment Variable**:
   - **Key:** `ConnectionStrings__DefaultConnection`
   - **Value:** Masukkan Connection String Supabase yang Anda peroleh di Langkah 1.
   - *(Opsional)* **Key:** `ASPNETCORE_ENVIRONMENT` -> **Value:** `Development` *(agar Swagger UI aktif)*.
7. Klik **Create Web Service**.
8. Render akan mulai build image Docker dan menjalankan API.
   - Proses build pertama kali membutuhkan waktu sekitar 2-3 menit.
   - Setelah selesai, status akan berubah menjadi **Live**.
9. **Tes Backend:**
   Buka URL Render Anda di browser (contoh: `https://gudangpro-api.onrender.com`). Anda akan langsung melihat halaman **Swagger UI (OpenAPI)** yang menampilkan seluruh 26 endpoint REST API!

---

## Langkah 3: Deploy Frontend ke Vercel

1. Buka [vercel.com](https://vercel.com) dan login dengan akun GitHub Anda.
2. Klik **Add New...** > **Project**.
3. Cari dan klik **Import** pada repositori `inventori-gudang`.
4. Pada halaman konfigurasi project:
   - **Project Name:** `gudangpro` (atau sesuai keinginan)
   - **Framework Preset:** Pilih **Angular**
   - **Root Directory:** Klik **Edit** dan pilih folder **`frontend`** > Klik **Continue**.
   - **Build and Output Settings:**
     - Build Command: `npm run build`
     - Output Directory: `dist/frontend/browser`
     - Install Command: `npm install`
5. Klik **Deploy**.
6. Vercel akan meng-compile Angular 20 dan dalam ~1 menit website Anda sudah online di alamat:
   `https://gudangpro.vercel.app` (atau nama project Anda).

---

## Langkah 4: Menghubungkan Frontend ke Backend (Bebas CORS!)

Agar Frontend di Vercel dapat berkomunikasi dengan Backend di Render tanpa kendala CORS:

1. Buka file `frontend/vercel.json` di project lokal Anda.
2. Masukkan URL backend Render Anda:
   ```json
   {
     "version": 2,
     "rewrites": [
       {
         "source": "/api/:match*",
         "destination": "https://gudangpro-api.onrender.com/api/:match*"
       },
       {
         "source": "/(.*)",
         "destination": "/index.html"
       }
     ]
   }
   ```
   *(Ganti `https://gudangpro-api.onrender.com` dengan URL backend Render Anda)*.
3. Commit dan push perubahan ke GitHub:
   ```bash
   git add frontend/vercel.json
   git commit -m "chore: configure api proxy to render backend"
   git push origin main
   ```
4. Vercel akan otomatis melakukan auto-redeploy dalam hitungan detik!

---

## 🔑 Pengujian & Akun Login Demo

Buka URL frontend Anda di Vercel (`https://nama-project.vercel.app`). Gunakan akun bawaan berikut:

| Peran | Username | Password | Hak Akses Utama |
|---|---|---|---|
| **Admin Gudang** | `admin` | `admin123` | Akses penuh seluruh sistem: Master Data, Stok, Transaksi, Pengguna, Audit Trail |
| **Staf Gudang** | `sari` | `sari123` | Operasional Barang Masuk, Barang Keluar, Riwayat Transaksi, Kartu Stok, Notifikasi |
| **Pemilik** | `hendra` | `hendra123` | Dashboard Eksekutif, Monitoring Transaksi, dan Ekspor Laporan |

---

## ⚡ Catatan Penting Mengenai Free Tier

1. **Cold Start Backend (Render Free Tier):**
   - Jika aplikasi tidak menerima kunjungan selama 15 menit, Render akan membuat server masuk ke mode *Sleep* (tidur).
   - Saat ada orang pertama kali membuka aplikasi lagi, request pertama akan butuh waktu sekitar **30-50 detik** untuk "membangunkan" container.
   - Setelah terbangun, aplikasi akan merespons dengan cepat seperti biasa.
   - *Tips:* Buka URL backend Render di browser terlebih dahulu sebelum mendemokan ke orang lain agar container sudah dalam keadaan aktif.

2. **Supabase Inactivity (Free Tier):**
   - Proyek gratis di Supabase akan di-pause otomatis jika tidak ada aktivitas database selama 1 minggu berturut-turut.
   - Jika ter-pause, cukup klik tombol *Restore* di dashboard Supabase dalam 1 klik.

3. **Alternatif Database Lain (Jika tidak ingin Supabase):**
   - **Neon.tech:** PostgreSQL Serverless 100% gratis, auto-wake instan (<1 detik). Cukup paste connection string Neon ke Render.
   - **MonsterASP.net:** Jika Anda ingin tetap menggunakan Microsoft SQL Server (MSSQL), MonsterASP.net memberikan database MSSQL 1GB gratis.
