-- ====================================================================
-- GudangPro — Supabase (PostgreSQL) Initial Schema & Seed Data
-- ====================================================================

-- 1. USERS
CREATE TABLE IF NOT EXISTS "Users" (
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
CREATE TABLE IF NOT EXISTS "Warehouses" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(20) NOT NULL UNIQUE,
    "Name" VARCHAR(100) NOT NULL,
    "Address" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- 3. USER WAREHOUSES
CREATE TABLE IF NOT EXISTS "UserWarehouses" (
    "UserId" INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    PRIMARY KEY ("UserId", "WarehouseId")
);

-- 4. CATEGORIES & UNITS
CREATE TABLE IF NOT EXISTS "Categories" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS "Units" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(20) NOT NULL UNIQUE
);

-- 5. ITEMS
CREATE TABLE IF NOT EXISTS "Items" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(30) NOT NULL UNIQUE,
    "Name" VARCHAR(150) NOT NULL,
    "CategoryId" INT NOT NULL REFERENCES "Categories"("Id") ON DELETE RESTRICT,
    "UnitId" INT NOT NULL REFERENCES "Units"("Id") ON DELETE RESTRICT,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- 6. STOCK LEVELS
CREATE TABLE IF NOT EXISTS "StockLevels" (
    "Id" SERIAL PRIMARY KEY,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    "Quantity" INT NOT NULL DEFAULT 0,
    "MinStock" INT NOT NULL DEFAULT 0,
    "RowVersion" BYTEA NULL,
    CONSTRAINT "UQ_StockLevel_Item_Warehouse" UNIQUE ("ItemId", "WarehouseId")
);

-- 7. STOCK TRANSACTIONS
CREATE TABLE IF NOT EXISTS "StockTransactions" (
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
CREATE TABLE IF NOT EXISTS "StockTransactionLines" (
    "Id" SERIAL PRIMARY KEY,
    "TransactionId" INT NOT NULL REFERENCES "StockTransactions"("Id") ON DELETE CASCADE,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE RESTRICT,
    "Quantity" INT NOT NULL
);

-- 9. ALERTS
CREATE TABLE IF NOT EXISTS "Alerts" (
    "Id" SERIAL PRIMARY KEY,
    "ItemId" INT NOT NULL REFERENCES "Items"("Id") ON DELETE CASCADE,
    "WarehouseId" INT NOT NULL REFERENCES "Warehouses"("Id") ON DELETE CASCADE,
    "Message" TEXT NOT NULL,
    "Level" VARCHAR(20) NOT NULL DEFAULT 'menipis',
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 10. AUDIT LOGS
CREATE TABLE IF NOT EXISTS "AuditLogs" (
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
-- SEED DATA (Akan di-insert jika tabel masih kosong)
-- ====================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Users" LIMIT 1) THEN
        -- Seed Users (BCrypt hashes for admin123, sari123, hendra123)
        INSERT INTO "Users" ("Id", "Username", "FullName", "Role", "PasswordHash", "IsActive", "LastLoginAt")
        VALUES 
        (1, 'admin', 'Budi Santoso', 'Admin Gudang', '$2a$11$N5J1c7Z20Vj7mXn7fT2eIOm6kPqmI01s3D8vS9X70qJt6k1c3u6iW', TRUE, NOW() - INTERVAL '2 hours'),
        (2, 'sari', 'Sari Wulandari', 'Staf Gudang', '$2a$11$r43lC1.jQYg2pX9.Zg9mfeIqP12jT6k7vV4eM2l9xN70qJt6k1c3u', TRUE, NOW() - INTERVAL '5 hours'),
        (3, 'hendra', 'Hendra Wijaya', 'Pemilik', '$2a$11$m28jR5.kXZl4pQ2.Ag7mfeLqP15jT8k9vW5eN3l8xO81qKt7l2d4v', TRUE, NOW() - INTERVAL '1 day');
        PERFORM setval(pg_get_serial_sequence('"Users"', 'Id'), 3);

        -- Seed Warehouses
        INSERT INTO "Warehouses" ("Id", "Code", "Name", "Address", "IsActive")
        VALUES 
        (1, 'GDG-UTM', 'Gudang Utama', 'Kawasan Industri MM2100 Blok C-4, Cikarang', TRUE),
        (2, 'GDG-BB', 'Gudang Bahan Baku', 'Jl. Raya Narogong Km 14, Bekasi', TRUE),
        (3, 'GDG-SC', 'Gudang Suku Cadang', 'Kawasan Jababeka II Blok J, Cikarang', TRUE),
        (4, 'GDG-BJ', 'Gudang Barang Jadi', 'Jl. Industri Raya No. 88, Karawang', TRUE);
        PERFORM setval(pg_get_serial_sequence('"Warehouses"', 'Id'), 4);

        -- Seed UserWarehouses
        INSERT INTO "UserWarehouses" ("UserId", "WarehouseId")
        VALUES (1, 1), (1, 2), (1, 3), (1, 4), (2, 1), (2, 2), (3, 1), (3, 4);

        -- Seed Categories & Units
        INSERT INTO "Categories" ("Id", "Name") VALUES (1, 'Bahan Baku'), (2, 'Barang Jadi'), (3, 'Kemasan'), (4, 'Suku Cadang');
        PERFORM setval(pg_get_serial_sequence('"Categories"', 'Id'), 4);

        INSERT INTO "Units" ("Id", "Name") VALUES (1, 'Pcs'), (2, 'Kg'), (3, 'Roll'), (4, 'Box'), (5, 'Liter');
        PERFORM setval(pg_get_serial_sequence('"Units"', 'Id'), 5);

        -- Seed Items
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
        PERFORM setval(pg_get_serial_sequence('"Items"', 'Id'), 8);

        -- Seed StockLevels
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
        PERFORM setval(pg_get_serial_sequence('"StockLevels"', 'Id'), 9);

        -- Seed Alerts
        INSERT INTO "Alerts" ("ItemId", "WarehouseId", "Level", "Message", "IsRead", "CreatedAt")
        VALUES 
        (4, 1, 'kritis', 'Stok Tangki Plastik 500L Biru kritis (3 / min 10 Pcs)', FALSE, NOW() - INTERVAL '1 hour'),
        (6, 2, 'kritis', 'Stok Pigmen Pewarna Biru kritis (8 / min 15 Kg)', FALSE, NOW() - INTERVAL '3 hours'),
        (2, 1, 'menipis', 'Stok Plastik PE Lembaran 0.5mm menipis (18 / min 20 Roll)', FALSE, NOW() - INTERVAL '6 hours'),
        (3, 1, 'menipis', 'Stok Karton Box mendekati batas aman', TRUE, NOW() - INTERVAL '1 day');

        -- Seed StockTransactions
        INSERT INTO "StockTransactions" ("Id", "TransactionNo", "Type", "WarehouseId", "TransactionDate", "ReferenceNo", "Notes", "CreatedBy", "CreatedAt")
        VALUES 
        (1, 'TRX-260923-001', 'IN', 1, NOW() - INTERVAL '1 day', 'SJ-SUP-2026-0089', 'Pengiriman rutin dari Supplier Utama', 'Budi Santoso', NOW() - INTERVAL '1 day'),
        (2, 'TRX-260924-002', 'OUT', 1, NOW(), 'WO-PRD-2026-0145', 'Pengeluaran untuk produksi batch 145', 'Sari Wulandari', NOW() - INTERVAL '2 hours');
        PERFORM setval(pg_get_serial_sequence('"StockTransactions"', 'Id'), 2);

        -- Seed StockTransactionLines
        INSERT INTO "StockTransactionLines" ("TransactionId", "ItemId", "Quantity")
        VALUES 
        (1, 1, 200),
        (1, 3, 300),
        (2, 2, 5);

        -- Seed AuditLogs
        INSERT INTO "AuditLogs" ("UserId", "Username", "UserFullName", "Action", "EntityName", "EntityId", "Detail", "IpAddress", "CreatedAt")
        VALUES 
        (1, 'admin', 'Budi Santoso', 'Membuat', 'Item', '1', 'Menambahkan barang baru: Biji Plastik PP Grade A', '127.0.0.1', NOW() - INTERVAL '2 days'),
        (1, 'admin', 'Budi Santoso', 'Membuat', 'Transaction', 'TRX-260923-001', 'Mencatat barang masuk nomor TRX-260923-001', '127.0.0.1', NOW() - INTERVAL '1 day'),
        (2, 'sari', 'Sari Wulandari', 'Membuat', 'Transaction', 'TRX-260924-002', 'Mencatat barang keluar nomor TRX-260924-002', '127.0.0.1', NOW() - INTERVAL '2 hours');
    END IF;
END $$;
