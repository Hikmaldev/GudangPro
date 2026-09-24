-- ====================================================================
-- GudangPro — Supabase (PostgreSQL) Initial Schema & Seed Data
-- ====================================================================

-- 0. BERSIHKAN TABEL LAMA (Jika ada tabel lama yang belum lengkap)
DROP TABLE IF EXISTS "AuditLogs" CASCADE;
DROP TABLE IF EXISTS "StockTransactionLines" CASCADE;
DROP TABLE IF EXISTS "StockTransactions" CASCADE;
DROP TABLE IF EXISTS "Alerts" CASCADE;
DROP TABLE IF EXISTS "StockLevels" CASCADE;
DROP TABLE IF EXISTS "Items" CASCADE;
DROP TABLE IF EXISTS "Categories" CASCADE;
DROP TABLE IF EXISTS "Units" CASCADE;
DROP TABLE IF EXISTS "UserWarehouses" CASCADE;
DROP TABLE IF EXISTS "Warehouses" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

-- 1. USERS
CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "FullName" VARCHAR(100) NOT NULL,
    "Role" VARCHAR(30) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "FailedLoginAttempts" INT NOT NULL DEFAULT 0,
    "LockoutEnd" TIMESTAMPTZ NULL,
    "LastLoginAt" TIMESTAMPTZ NULL
);

-- 2. WAREHOUSES
CREATE TABLE "Warehouses" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(20) NOT NULL UNIQUE,
    "Name" VARCHAR(100) NOT NULL,
    "Address" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- 3. USER WAREHOUSES
CREATE TABLE "UserWarehouses" (
    "UserId" INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("UserId", "WarehouseId")
);

-- 4. CATEGORIES & UNITS
CREATE TABLE "Categories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE "Units" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(20) NOT NULL UNIQUE
);

-- 5. ITEMS
CREATE TABLE "Items" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(30) NOT NULL UNIQUE,
    "Name" VARCHAR(150) NOT NULL,
    "CategoryId" INT NOT NULL REFERENCES "Categories"("Id") ON DELETE RESTRICT,
    "UnitId" INT NOT NULL REFERENCES "Units"("Id") ON DELETE RESTRICT,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- 6. STOCK LEVELS
CREATE TABLE "StockLevels" (
    "Id" SERIAL PRIMARY KEY,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    "Quantity" INT NOT NULL DEFAULT 0,
    "MinStock" INT NOT NULL DEFAULT 0,
    "RowVersion" BYTEA NULL,
    CONSTRAINT "UQ_StockLevel_Item_Warehouse" UNIQUE ("ItemId", "WarehouseId")
);

-- 7. STOCK TRANSACTIONS
CREATE TABLE "StockTransactions" (
    "Id" SERIAL PRIMARY KEY,
    "TransactionNo" VARCHAR(40) NOT NULL UNIQUE,
    "Type" VARCHAR(10) NOT NULL,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE RESTRICT,
    "TransactionDate" TIMESTAMPTZ NOT NULL,
    "ReferenceNo" VARCHAR(50) NOT NULL,
    "Notes" TEXT NULL,
    "CreatedBy" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "CancelledOfId" INT NULL REFERENCES "StockTransactions"("Id") ON DELETE RESTRICT,
    "CancellationReason" TEXT NULL,
    "IsCancelled" BOOLEAN NOT NULL DEFAULT FALSE
);

-- 8. STOCK TRANSACTION LINES
CREATE TABLE "StockTransactionLines" (
    "Id" SERIAL PRIMARY KEY,
    "TransactionId" INT NOT NULL REFERENCES "StockTransactions"("Id") ON DELETE CASCADE,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE RESTRICT,
    "Quantity" INT NOT NULL
);

-- 9. ALERTS
CREATE TABLE "Alerts" (
    "Id" SERIAL PRIMARY KEY,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    "Message" TEXT NOT NULL,
    "Level" VARCHAR(20) NOT NULL DEFAULT 'menipis',
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 10. AUDIT LOGS
CREATE TABLE "AuditLogs" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
    "UserFullName" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(50) NOT NULL,
    "Action" VARCHAR(30) NOT NULL,
    "EntityName" VARCHAR(50) NOT NULL,
    "EntityId" VARCHAR(50) NULL,
    "OldValue" TEXT NULL,
    "NewValue" TEXT NULL,
    "Detail" TEXT NULL,
    "IpAddress" VARCHAR(50) NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ====================================================================
-- SEED DATA
-- ====================================================================

-- 1. Users (BCrypt hashes terverifikasi: admin123, sari123, hendra123)
INSERT INTO "Users" ("Id", "Username", "FullName", "Role", "PasswordHash", "IsActive", "LastLoginAt")
VALUES 
(1, 'admin', 'Budi Santoso', 'Admin Gudang', '$2a$11$M32NodrFIfHt1DN8M3xun.nqpmchvhUgbT81c/TOqd0kjUAZ1usvS', TRUE, NOW() - INTERVAL '2 hours'),
(2, 'sari', 'Sari Wulandari', 'Staf Gudang', '$2a$11$K6r17DguVUp7bQlwtGtzheSzANCxTNKF2RcNUFakIGdnxejDoqAti', TRUE, NOW() - INTERVAL '5 hours'),
(3, 'hendra', 'Hendra Wijaya', 'Pemilik', '$2a$11$xu/guuEr1K8QR1aGLvqvC.49mJjYobZVs0OBtiE7zh5R3AqgG1bwy', TRUE, NOW() - INTERVAL '1 day');

-- 2. Warehouses
INSERT INTO "Warehouses" ("Id", "Code", "Name", "Address", "IsActive")
VALUES 
(1, 'GDG-UTM', 'Gudang Utama', 'Kawasan Industri MM2100 Blok C-4, Cikarang', TRUE),
(2, 'GDG-BB', 'Gudang Bahan Baku', 'Jl. Raya Narogong Km 14, Bekasi', TRUE),
(3, 'GDG-SC', 'Gudang Suku Cadang', 'Kawasan Jababeka II Blok J, Cikarang', TRUE),
(4, 'GDG-BJ', 'Gudang Barang Jadi', 'Jl. Industri Raya No. 88, Karawang', TRUE);

-- 3. UserWarehouses
INSERT INTO "UserWarehouses" ("UserId", "WarehouseId")
VALUES 
(1, 1), (1, 2), (1, 3), (1, 4),
(2, 1), (2, 2),
(3, 1), (3, 4);

-- 4. Categories & Units
INSERT INTO "Categories" ("Id", "Name") 
VALUES 
(1, 'Bahan Baku'), 
(2, 'Barang Jadi'), 
(3, 'Kemasan'), 
(4, 'Suku Cadang');

INSERT INTO "Units" ("Id", "Name") 
VALUES 
(1, 'Pcs'), 
(2, 'Kg'), 
(3, 'Roll'), 
(4, 'Box'), 
(5, 'Liter');

-- 5. Items
INSERT INTO "Items" ("Id", "Code", "Name", "CategoryId", "UnitId", "IsActive")
VALUES 
(1, 'BB-RS-001', 'Biji Plastik PP Grade A', 1, 2, TRUE),
(2, 'BB-PE-004', 'Plastik PE Lembaran 0.5mm', 1, 3, TRUE),
(3, 'KM-BX-012', 'Karton Box 40x30x20cm', 3, 1, TRUE),
(4, 'BJ-TG-001', 'Tangki Plastik 500L Biru', 2, 1, TRUE),
(5, 'SC-BL-003', 'Baut Baja M8x40 (Pack)', 4, 4, TRUE),
(6, 'BB-PG-002', 'Pigmen Pewarna Biru', 1, 2, TRUE),
(7, 'KM-ST-005', 'Stiker Label Garansi', 3, 3, TRUE),
(8, 'SC-SL-008', 'Seal Karet EPDM 2 inch', 4, 1, FALSE);

-- 6. StockLevels
INSERT INTO "StockLevels" ("Id", "ItemId", "WarehouseId", "Quantity", "MinStock")
VALUES 
(1, 1, 1, 450, 100),
(2, 1, 2, 1200, 200),
(3, 2, 1, 18, 20),
(4, 3, 1, 820, 150),
(5, 4, 1, 3, 10),
(6, 5, 1, 45, 20),
(7, 6, 2, 8, 15),
(8, 7, 1, 95, 50),
(9, 8, 3, 0, 25);

-- 7. Alerts
INSERT INTO "Alerts" ("ItemId", "WarehouseId", "Level", "Message", "IsRead", "CreatedAt")
VALUES 
(4, 1, 'kritis', 'Stok Tangki Plastik 500L Biru kritis (3 / min 10 Pcs)', FALSE, NOW() - INTERVAL '1 hour'),
(6, 2, 'kritis', 'Stok Pigmen Pewarna Biru kritis (8 / min 15 Kg)', FALSE, NOW() - INTERVAL '3 hours'),
(2, 1, 'menipis', 'Stok Plastik PE Lembaran 0.5mm menipis (18 / min 20 Roll)', FALSE, NOW() - INTERVAL '6 hours'),
(3, 1, 'menipis', 'Stok Karton Box mendekati batas aman', TRUE, NOW() - INTERVAL '1 day');

-- 8. StockTransactions
INSERT INTO "StockTransactions" ("Id", "TransactionNo", "Type", "WarehouseId", "TransactionDate", "ReferenceNo", "Notes", "CreatedBy", "CreatedAt")
VALUES 
(1, 'TRX-260923-001', 'IN', 1, NOW() - INTERVAL '1 day', 'SJ-SUP-2026-0089', 'Pengiriman rutin dari Supplier Utama', 'Budi Santoso', NOW() - INTERVAL '1 day'),
(2, 'TRX-260924-002', 'OUT', 1, NOW(), 'WO-PRD-2026-0145', 'Pengeluaran untuk produksi batch 145', 'Sari Wulandari', NOW() - INTERVAL '2 hours');

-- 9. StockTransactionLines
INSERT INTO "StockTransactionLines" ("TransactionId", "ItemId", "Quantity")
VALUES 
(1, 1, 200),
(1, 3, 300),
(2, 2, 5);

-- 10. AuditLogs
INSERT INTO "AuditLogs" ("UserId", "Username", "UserFullName", "Action", "EntityName", "EntityId", "Detail", "IpAddress", "CreatedAt")
VALUES 
(1, 'admin', 'Budi Santoso', 'Membuat', 'Item', '1', 'Menambahkan barang baru: Biji Plastik PP Grade A', '127.0.0.1', NOW() - INTERVAL '2 days'),
(1, 'admin', 'Budi Santoso', 'Membuat', 'Transaction', 'TRX-260923-001', 'Mencatat barang masuk nomor TRX-260923-001', '127.0.0.1', NOW() - INTERVAL '1 day'),
(2, 'sari', 'Sari Wulandari', 'Membuat', 'Transaction', 'TRX-260924-002', 'Mencatat barang keluar nomor TRX-260924-002', '127.0.0.1', NOW() - INTERVAL '2 hours');

-- ====================================================================
-- SINKRONISASI SEQUENCE (Agar insert data baru tidak bentrok Id)
-- ====================================================================
SELECT setval(pg_get_serial_sequence('public."Users"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Users"), 1));
SELECT setval(pg_get_serial_sequence('public."Warehouses"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Warehouses"), 1));
SELECT setval(pg_get_serial_sequence('public."Categories"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Categories"), 1));
SELECT setval(pg_get_serial_sequence('public."Units"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Units"), 1));
SELECT setval(pg_get_serial_sequence('public."Items"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Items"), 1));
SELECT setval(pg_get_serial_sequence('public."StockLevels"', 'Id'), COALESCE((SELECT MAX("Id") FROM "StockLevels"), 1));
SELECT setval(pg_get_serial_sequence('public."StockTransactions"', 'Id'), COALESCE((SELECT MAX("Id") FROM "StockTransactions"), 1));
SELECT setval(pg_get_serial_sequence('public."StockTransactionLines"', 'Id'), COALESCE((SELECT MAX("Id") FROM "StockTransactionLines"), 1));
SELECT setval(pg_get_serial_sequence('public."Alerts"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Alerts"), 1));
SELECT setval(pg_get_serial_sequence('public."AuditLogs"', 'Id'), COALESCE((SELECT MAX("Id") FROM "AuditLogs"), 1));
