# Product Requirements Document (PRD)

## Sistem Manajemen Inventori Gudang untuk UKM Manufaktur

| Item | Keterangan |
|---|---|
| Versi dokumen | 1.0 (Draft) |
| Tanggal | 23 September 2026 |
| Status | Draft untuk review |
| Tech stack | C#, .NET 6+, Angular, SQL Server, REST API |

---

## 1. Ringkasan Produk

Sistem Manajemen Inventori Gudang adalah aplikasi web untuk mencatat dan memantau stok barang di UKM manufaktur. Sistem ini menggantikan pencatatan manual di Excel. Sistem menyimpan semua transaksi barang masuk dan keluar dalam satu basis data terpusat. Sistem juga menghitung stok secara otomatis dan memberi peringatan saat stok menipis.

## 2. Latar Belakang dan Masalah

UKM manufaktur umumnya mencatat stok di file Excel. Cara ini menimbulkan beberapa masalah:

- Beberapa orang mengedit file yang sama dan data saling menimpa.
- Staf salah ketik jumlah atau kode barang.
- Angka stok di Excel berbeda dengan stok fisik.
- Tidak ada jejak siapa mengubah data dan kapan.
- Pemilik tidak bisa melihat stok terkini tanpa menanyakan ke staf gudang.
- Tim baru tahu stok habis setelah produksi terhambat.

## 3. Tujuan dan Metrik Keberhasilan

### 3.1 Tujuan

1. Menghilangkan pencatatan stok manual di Excel.
2. Mengurangi selisih antara stok sistem dan stok fisik.
3. Memberi peringatan dini sebelum stok habis.
4. Membuat setiap perubahan stok dapat ditelusuri.
5. Membatasi akses data sesuai peran pengguna.

### 3.2 Metrik Keberhasilan

| Metrik | Target | Cara Ukur |
|---|---|---|
| Selisih stok sistem vs fisik saat stock opname | Kurang dari 2% | Hasil stock opname bulanan |
| Waktu mencatat satu transaksi | Kurang dari 1 menit | Uji langsung dengan staf gudang |
| Kejadian stok habis tanpa peringatan | 0 per bulan | Laporan alert vs kejadian aktual |
| Adopsi pengguna | 90% transaksi tercatat di sistem dalam 2 bulan | Log transaksi |
| Waktu respons halaman utama | Kurang dari 2 detik | Monitoring performa |

## 4. Ruang Lingkup

### 4.1 Dalam Lingkup (In Scope)

- Manajemen data master barang, kategori, satuan, dan gudang.
- Pencatatan barang masuk dan barang keluar.
- Validasi stok minimum dan stok tidak boleh negatif.
- Riwayat transaksi per gudang atau cabang.
- Dashboard grafik stok dan alert stok menipis.
- Role-based access untuk admin gudang dan staf.
- Ekspor laporan ke Excel dan PDF.
- Audit log perubahan data.

### 4.2 Di Luar Lingkup (Out of Scope) untuk Rilis Pertama

- Modul pembelian dan penjualan penuh.
- Integrasi dengan software akuntansi.
- Pemindaian barcode dengan aplikasi mobile native.
- Perhitungan harga pokok (HPP) dan valuasi persediaan.
- Manajemen produksi dan Bill of Materials (BOM).

## 5. Target Pengguna

### 5.1 Persona

**Admin Gudang (Budi, 38 tahun)**
- Mengelola data master dan mengawasi stok.
- Perlu melihat kondisi semua gudang dalam satu layar.
- Perlu mengatur akun staf dan menentukan stok minimum.

**Staf Gudang (Sari, 25 tahun)**
- Mencatat barang yang datang dan keluar setiap hari.
- Perlu input cepat dengan langkah sedikit.
- Perlu tahu jika stok tidak cukup sebelum mengeluarkan barang.

**Pemilik atau Manajer (opsional, akses baca)**
- Perlu ringkasan stok dan laporan tanpa mengubah data.

## 6. Peran dan Hak Akses

| Fitur | Admin Gudang | Staf |
|---|---|---|
| Lihat dashboard | Ya | Ya (gudang yang ditugaskan) |
| Kelola data master barang | Ya | Tidak |
| Kelola gudang dan cabang | Ya | Tidak |
| Catat barang masuk | Ya | Ya |
| Catat barang keluar | Ya | Ya |
| Edit atau batalkan transaksi | Ya | Tidak |
| Lihat riwayat transaksi | Semua gudang | Gudang yang ditugaskan |
| Atur stok minimum | Ya | Tidak |
| Kelola pengguna dan peran | Ya | Tidak |
| Ekspor laporan | Ya | Terbatas |
| Lihat audit log | Ya | Tidak |

## 7. Kebutuhan Fungsional

### 7.1 Autentikasi dan Otorisasi

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-AUTH-01 | Pengguna login dengan username dan password. | Must |
| FR-AUTH-02 | Sistem memakai JWT dengan refresh token. | Must |
| FR-AUTH-03 | Sistem membatasi akses endpoint dan menu berdasarkan peran. | Must |
| FR-AUTH-04 | Sistem mengunci akun sementara setelah 5 kali gagal login. | Should |
| FR-AUTH-05 | Admin dapat mengatur ulang password pengguna. | Should |

### 7.2 Manajemen Data Master

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-MST-01 | Admin dapat menambah, mengubah, menonaktifkan, dan melihat barang (kode, nama, kategori, satuan, stok minimum). | Must |
| FR-MST-02 | Kode barang bersifat unik. | Must |
| FR-MST-03 | Admin dapat mengelola gudang atau cabang (kode, nama, alamat). | Must |
| FR-MST-04 | Admin dapat mengelola kategori dan satuan. | Should |
| FR-MST-05 | Sistem tidak menghapus barang yang sudah punya transaksi. Sistem hanya menonaktifkannya. | Must |
| FR-MST-06 | Admin dapat mengimpor data barang awal dari file Excel. | Should |

### 7.3 Barang Masuk

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-IN-01 | Pengguna dapat membuat transaksi barang masuk dengan gudang tujuan, tanggal, referensi (mis. nomor surat jalan), dan daftar barang beserta jumlah. | Must |
| FR-IN-02 | Sistem menambah stok gudang tujuan saat transaksi disimpan. | Must |
| FR-IN-03 | Sistem menolak jumlah nol atau negatif. | Must |
| FR-IN-04 | Pengguna dapat menambahkan catatan pada transaksi. | Should |
| FR-IN-05 | Admin dapat membatalkan transaksi dengan alasan. Sistem membuat transaksi koreksi. | Must |

### 7.4 Barang Keluar

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-OUT-01 | Pengguna dapat membuat transaksi barang keluar dengan gudang asal, tanggal, tujuan atau referensi, dan daftar barang beserta jumlah. | Must |
| FR-OUT-02 | Sistem menolak transaksi jika jumlah keluar melebihi stok tersedia. | Must |
| FR-OUT-03 | Sistem menampilkan peringatan jika stok setelah transaksi berada di bawah stok minimum. | Must |
| FR-OUT-04 | Admin dapat mengatur apakah transaksi di bawah stok minimum perlu persetujuan admin. | Could |
| FR-OUT-05 | Admin dapat membatalkan transaksi dengan alasan. Sistem membuat transaksi koreksi. | Must |

### 7.5 Validasi Stok Minimum

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-VAL-01 | Setiap barang memiliki nilai stok minimum, dapat berbeda per gudang. | Must |
| FR-VAL-02 | Sistem menjalankan validasi di sisi server. Validasi di Angular hanya membantu pengalaman pengguna. | Must |
| FR-VAL-03 | Sistem mencegah stok bernilai negatif, termasuk saat dua pengguna mengeluarkan barang yang sama secara bersamaan (lihat bagian 9.3). | Must |

### 7.6 Riwayat Transaksi

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-HIS-01 | Pengguna dapat melihat riwayat transaksi per gudang atau cabang. | Must |
| FR-HIS-02 | Pengguna dapat memfilter berdasarkan rentang tanggal, jenis transaksi, barang, dan pembuat transaksi. | Must |
| FR-HIS-03 | Sistem menampilkan data dengan pagination dan pengurutan. | Must |
| FR-HIS-04 | Pengguna dapat mencari berdasarkan kode atau nama barang dan nomor referensi. | Should |
| FR-HIS-05 | Pengguna dapat melihat kartu stok per barang (saldo awal, masuk, keluar, saldo akhir). | Should |
| FR-HIS-06 | Pengguna dengan izin dapat mengekspor riwayat ke Excel dan PDF. | Should |

### 7.7 Dashboard dan Alert

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-DSH-01 | Dashboard menampilkan total jenis barang, total transaksi hari ini, dan jumlah barang di bawah stok minimum. | Must |
| FR-DSH-02 | Dashboard menampilkan grafik stok per kategori atau per gudang. | Must |
| FR-DSH-03 | Dashboard menampilkan grafik tren barang masuk dan keluar (harian, mingguan, bulanan). | Must |
| FR-DSH-04 | Dashboard menampilkan daftar barang dengan stok menipis, diurutkan dari yang paling kritis. | Must |
| FR-DSH-05 | Sistem menampilkan notifikasi dalam aplikasi saat stok barang turun sampai atau di bawah stok minimum. | Must |
| FR-DSH-06 | Sistem mengirim ringkasan alert harian melalui email ke admin. | Could |
| FR-DSH-07 | Pengguna dapat memfilter dashboard per gudang. | Should |

### 7.8 Audit Log

| ID | Kebutuhan | Prioritas |
|---|---|---|
| FR-AUD-01 | Sistem mencatat siapa, kapan, dan apa yang berubah pada data master dan transaksi. | Must |
| FR-AUD-02 | Admin dapat melihat dan memfilter audit log. | Should |
| FR-AUD-03 | Pengguna tidak dapat mengubah atau menghapus audit log. | Must |

## 8. Alur Pengguna Utama

### 8.1 Mencatat Barang Keluar

1. Staf login.
2. Staf membuka menu Barang Keluar.
3. Staf memilih gudang asal dan mengisi tanggal serta referensi.
4. Staf menambahkan barang dan jumlah.
5. Sistem menampilkan stok tersedia di setiap baris.
6. Staf menekan Simpan.
7. Sistem memvalidasi stok di server.
8. Jika stok cukup, sistem menyimpan transaksi dan mengurangi stok.
9. Jika stok jatuh di bawah minimum, sistem menampilkan peringatan dan membuat alert.
10. Jika stok tidak cukup, sistem menolak transaksi dan menampilkan pesan yang jelas.

### 8.2 Memantau Stok

1. Admin login dan membuka dashboard.
2. Admin melihat jumlah barang menipis dan grafik stok.
3. Admin membuka daftar barang menipis.
4. Admin memilih barang untuk melihat kartu stok dan riwayat transaksinya.

## 9. Kebutuhan Non-Fungsional

### 9.1 Performa

- Halaman utama termuat dalam waktu kurang dari 2 detik pada jaringan kantor normal.
- API merespons kurang dari 500 ms untuk 95% permintaan baca.
- Sistem mendukung minimal 50 pengguna aktif bersamaan.
- Sistem menangani minimal 500.000 baris transaksi tanpa penurunan performa yang berarti.

### 9.2 Keamanan

- Sistem menyimpan password dengan hashing yang kuat (mis. BCrypt atau ASP.NET Core Identity).
- Sistem memakai HTTPS untuk semua komunikasi.
- Sistem memvalidasi semua input di server.
- Sistem memakai query berparameter melalui Entity Framework Core untuk mencegah SQL injection.
- Sistem mengaktifkan CORS hanya untuk domain frontend yang diizinkan.
- Sistem tidak menyimpan rahasia (connection string, kunci JWT) di repositori kode.

### 9.3 Integritas Data

- Setiap transaksi berjalan dalam satu database transaction.
- Sistem memakai concurrency control (mis. `RowVersion` pada tabel stok) untuk mencegah stok negatif akibat transaksi bersamaan.
- Sistem tidak menghapus transaksi secara fisik. Koreksi dilakukan lewat transaksi pembalik.

### 9.4 Ketersediaan dan Backup

- Target ketersediaan 99% pada jam kerja.
- Backup database penuh harian dan backup log transaksi berkala.
- Uji pemulihan backup minimal setiap kuartal.

### 9.5 Kegunaan

- Antarmuka berbahasa Indonesia.
- Tata letak responsif untuk desktop dan tablet.
- Form transaksi dapat diisi dengan keyboard tanpa mouse.
- Pesan galat jelas dan menyebut cara memperbaikinya.

### 9.6 Kompatibilitas

- Mendukung versi terbaru Chrome, Edge, dan Firefox.

### 9.7 Maintainability

- Kode mengikuti arsitektur berlapis (API, Application, Domain, Infrastructure).
- Unit test untuk logika bisnis stok. Target cakupan 70% pada layer Application.
- Dokumentasi API memakai Swagger atau OpenAPI.

## 10. Arsitektur Teknis

### 10.1 Stack

| Lapisan | Teknologi |
|---|---|
| Frontend | Angular (versi LTS terbaru), Angular Material, Chart.js atau ngx-charts |
| Backend | C#, ASP.NET Core Web API (.NET 6+) |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Autentikasi | ASP.NET Core Identity dan JWT |
| Dokumentasi API | Swagger (Swashbuckle) |
| Logging | Serilog |
| Testing | xUnit (backend), Jasmine dan Karma atau Jest (frontend) |

### 10.2 Struktur Komponen

```
[Angular SPA] <--HTTPS/REST--> [ASP.NET Core Web API] <--EF Core--> [SQL Server]
                                        |
                                   [Serilog / Email service]
```

### 10.3 Model Data (Ringkas)

| Tabel | Kolom Utama |
|---|---|
| Users | Id, Username, PasswordHash, FullName, Role, IsActive |
| Warehouses | Id, Code, Name, Address, IsActive |
| UserWarehouses | UserId, WarehouseId |
| Categories | Id, Name |
| Units | Id, Name |
| Items | Id, Code, Name, CategoryId, UnitId, IsActive |
| StockLevels | Id, ItemId, WarehouseId, Quantity, MinStock, RowVersion |
| StockTransactions | Id, TransactionNo, Type (IN/OUT/ADJUST), WarehouseId, TransactionDate, ReferenceNo, Notes, CreatedBy, CreatedAt, CancelledOfId |
| StockTransactionLines | Id, TransactionId, ItemId, Quantity |
| Alerts | Id, ItemId, WarehouseId, Message, IsRead, CreatedAt |
| AuditLogs | Id, UserId, Action, EntityName, EntityId, OldValue, NewValue, CreatedAt |

### 10.4 Daftar Endpoint REST (Ringkas)

| Metode | Endpoint | Fungsi | Peran |
|---|---|---|---|
| POST | `/api/auth/login` | Login | Semua |
| POST | `/api/auth/refresh` | Perbarui token | Semua |
| GET | `/api/items` | Daftar barang | Semua |
| POST | `/api/items` | Tambah barang | Admin |
| PUT | `/api/items/{id}` | Ubah barang | Admin |
| PATCH | `/api/items/{id}/deactivate` | Nonaktifkan barang | Admin |
| GET | `/api/warehouses` | Daftar gudang | Semua |
| POST | `/api/warehouses` | Tambah gudang | Admin |
| POST | `/api/transactions/in` | Catat barang masuk | Admin, Staf |
| POST | `/api/transactions/out` | Catat barang keluar | Admin, Staf |
| POST | `/api/transactions/{id}/cancel` | Batalkan transaksi | Admin |
| GET | `/api/transactions` | Riwayat (filter: gudang, tanggal, jenis, barang) | Semua |
| GET | `/api/stock` | Stok per barang dan gudang | Semua |
| GET | `/api/stock/{itemId}/card` | Kartu stok | Semua |
| GET | `/api/dashboard/summary` | Ringkasan dashboard | Semua |
| GET | `/api/dashboard/charts` | Data grafik | Semua |
| GET | `/api/alerts` | Daftar alert | Semua |
| PATCH | `/api/alerts/{id}/read` | Tandai alert dibaca | Semua |
| GET | `/api/users` | Daftar pengguna | Admin |
| POST | `/api/users` | Tambah pengguna | Admin |
| GET | `/api/audit-logs` | Audit log | Admin |
| GET | `/api/reports/export` | Ekspor laporan | Admin |

### 10.5 Aturan Bisnis Inti

1. Stok hanya berubah lewat transaksi. Tidak ada edit langsung pada jumlah stok.
2. Stok tidak boleh negatif.
3. Transaksi yang sudah disimpan tidak dihapus. Admin membatalkannya lewat transaksi koreksi.
4. Alert muncul saat `Quantity <= MinStock` setelah transaksi keluar.
5. Staf hanya mengakses gudang yang ditugaskan kepadanya.

## 11. Kebutuhan Antarmuka (UI)

### 11.1 Halaman Utama

| Halaman | Isi Utama |
|---|---|
| Login | Form username dan password |
| Dashboard | Kartu ringkasan, grafik stok, grafik tren, daftar stok menipis |
| Data Barang | Tabel, pencarian, form tambah dan ubah |
| Data Gudang | Tabel dan form gudang atau cabang |
| Barang Masuk | Form transaksi dan daftar transaksi |
| Barang Keluar | Form transaksi dan daftar transaksi |
| Riwayat Transaksi | Tabel dengan filter dan ekspor |
| Kartu Stok | Riwayat mutasi per barang |
| Notifikasi | Daftar alert |
| Manajemen Pengguna | Tabel pengguna, peran, dan gudang yang ditugaskan |
| Audit Log | Tabel log dengan filter |

### 11.2 Prinsip Desain

- Tampilan bersih dan tidak ramai.
- Warna status konsisten: merah untuk stok kritis, kuning untuk menipis, hijau untuk aman.
- Tabel besar memakai pagination sisi server.
- Tombol aksi utama selalu terlihat.

## 12. Rencana Rilis

| Fase | Cakupan | Perkiraan Durasi |
|---|---|---|
| Fase 0 | Analisis kebutuhan, desain UI, desain database | 2 minggu |
| Fase 1 (MVP) | Autentikasi, role, data master, barang masuk dan keluar, validasi stok | 5 minggu |
| Fase 2 | Riwayat transaksi, kartu stok, ekspor laporan | 3 minggu |
| Fase 3 | Dashboard, grafik, alert stok menipis, audit log | 3 minggu |
| Fase 4 | UAT, perbaikan bug, migrasi data dari Excel, deployment, pelatihan | 3 minggu |

Total perkiraan: 16 minggu untuk satu tim kecil. Angka ini perlu divalidasi setelah ukuran tim ditentukan.

## 13. Strategi Pengujian

- **Unit test:** logika stok, validasi, dan aturan alert.
- **Integration test:** endpoint API dengan database uji.
- **Uji konkurensi:** dua transaksi keluar bersamaan pada barang yang sama.
- **UAT:** admin dan staf gudang menjalankan skenario harian.
- **Uji migrasi:** bandingkan hasil impor Excel dengan data sumber.

## 14. Migrasi dan Deployment

- Admin menyiapkan file Excel stok awal sesuai template impor.
- Tim melakukan stock opname sebelum go-live untuk menetapkan saldo awal.
- Tim menjalankan sistem baru dan Excel secara paralel selama 1 sampai 2 minggu.
- Tim menghentikan Excel setelah selisih data terkendali.
- Deployment awal memakai IIS atau Docker di server lokal atau cloud. Tim memilih opsi berdasarkan kondisi infrastruktur klien.

## 15. Risiko dan Mitigasi

| Risiko | Dampak | Mitigasi |
|---|---|---|
| Staf enggan meninggalkan Excel | Adopsi rendah | Pelatihan singkat, form input cepat, masa paralel |
| Data awal tidak akurat | Stok sistem salah sejak awal | Stock opname sebelum go-live |
| Transaksi bersamaan menyebabkan stok salah | Data tidak konsisten | Concurrency control dan database transaction |
| Internet kantor tidak stabil | Pencatatan terhambat | Opsi hosting lokal (on-premise) |
| Cakupan proyek melebar | Jadwal molor | Kunci cakupan MVP, tampung permintaan baru di backlog |
| Kebocoran akun | Data dimanipulasi | Password kuat, kunci akun, audit log |

## 16. Asumsi dan Ketergantungan

**Asumsi**
- Setiap gudang memiliki komputer atau tablet dengan akses jaringan.
- Satu barang memakai satu satuan dasar.
- Pengguna tidak memerlukan mode offline pada rilis pertama.

**Ketergantungan**
- Klien menyediakan data barang dan stok awal.
- Klien menyediakan server atau akun cloud.
- Klien menyediakan layanan email (SMTP) jika fitur email alert dipakai.

## 17. Pengembangan Masa Depan (Backlog)

- Transfer stok antar gudang dengan status kirim dan terima.
- Pemindaian barcode atau QR code.
- Stock opname langsung di aplikasi.
- Modul pembelian, supplier, dan purchase order.
- Valuasi persediaan (FIFO atau rata-rata).
- Notifikasi lewat WhatsApp atau Telegram.
- Aplikasi mobile atau PWA dengan mode offline.
- Integrasi dengan software akuntansi.

## 18. Pertanyaan Terbuka

1. Berapa jumlah gudang atau cabang dan jumlah pengguna pada tahap awal?
2. Apakah satu barang perlu beberapa satuan (mis. dus dan pcs)?
3. Apakah barang perlu nomor batch atau tanggal kedaluwarsa?
4. Apakah transaksi di bawah stok minimum perlu persetujuan admin?
5. Di mana sistem akan di-hosting (lokal atau cloud)?
6. Format laporan apa yang sudah dipakai klien saat ini?

## 19. Glosarium

| Istilah | Arti |
|---|---|
| Stok minimum | Batas jumlah stok terendah sebelum sistem memberi alert |
| Kartu stok | Catatan mutasi masuk, keluar, dan saldo per barang |
| Stock opname | Perhitungan fisik stok untuk mencocokkan dengan data sistem |
| Transaksi koreksi | Transaksi pembalik yang membatalkan efek transaksi sebelumnya |
| RBAC | Role-Based Access Control, pembatasan akses berdasarkan peran |
| UAT | User Acceptance Testing, uji penerimaan oleh pengguna |
