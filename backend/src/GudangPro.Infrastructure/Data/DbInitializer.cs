using GudangPro.Domain.Entities;
using GudangPro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // 1. Users (BCrypt hash)
        // admin / admin123 (Admin Gudang)
        // sari / sari123 (Staf Gudang)
        // hendra / hendra123 (Pemilik)
        var admin = new User
        {
            Username = "admin",
            FullName = "Budi Santoso",
            Role = "Admin Gudang",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddHours(-2),
        };

        var staf = new User
        {
            Username = "sari",
            FullName = "Sari Wulandari",
            Role = "Staf Gudang",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("sari123"),
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddHours(-5),
        };

        var owner = new User
        {
            Username = "hendra",
            FullName = "Hendra Wijaya",
            Role = "Pemilik",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("hendra123"),
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
        };

        context.Users.AddRange(admin, staf, owner);
        await context.SaveChangesAsync();

        // 2. Warehouses
        var whUtama = new Warehouse { Code = "GDG-UTM", Name = "Gudang Utama", Address = "Kawasan Industri MM2100 Blok C-4, Cikarang", IsActive = true };
        var whBB = new Warehouse { Code = "GDG-BB", Name = "Gudang Bahan Baku", Address = "Jl. Raya Narogong Km 14, Bekasi", IsActive = true };
        var whSC = new Warehouse { Code = "GDG-SC", Name = "Gudang Suku Cadang", Address = "Kawasan Jababeka II Blok J, Cikarang", IsActive = true };
        var whBJ = new Warehouse { Code = "GDG-BJ", Name = "Gudang Barang Jadi", Address = "Jl. Industri Raya No. 88, Karawang", IsActive = true };

        context.Warehouses.AddRange(whUtama, whBB, whSC, whBJ);
        await context.SaveChangesAsync();

        // Assign warehouses to users
        context.UserWarehouses.AddRange(
            new UserWarehouse { UserId = admin.Id, WarehouseId = whUtama.Id },
            new UserWarehouse { UserId = admin.Id, WarehouseId = whBB.Id },
            new UserWarehouse { UserId = admin.Id, WarehouseId = whSC.Id },
            new UserWarehouse { UserId = admin.Id, WarehouseId = whBJ.Id },
            new UserWarehouse { UserId = staf.Id, WarehouseId = whUtama.Id },
            new UserWarehouse { UserId = staf.Id, WarehouseId = whBB.Id },
            new UserWarehouse { UserId = owner.Id, WarehouseId = whUtama.Id },
            new UserWarehouse { UserId = owner.Id, WarehouseId = whBJ.Id }
        );

        // 3. Categories & Units
        var catBB = new Category { Name = "Bahan Baku" };
        var catBJ = new Category { Name = "Barang Jadi" };
        var catKemasan = new Category { Name = "Kemasan" };
        var catSC = new Category { Name = "Suku Cadang" };
        context.Categories.AddRange(catBB, catBJ, catKemasan, catSC);

        var unitPcs = new Unit { Name = "Pcs" };
        var unitKg = new Unit { Name = "Kg" };
        var unitRoll = new Unit { Name = "Roll" };
        var unitBox = new Unit { Name = "Box" };
        var unitLiter = new Unit { Name = "Liter" };
        context.Units.AddRange(unitPcs, unitKg, unitRoll, unitBox, unitLiter);
        await context.SaveChangesAsync();

        // 4. Items
        var items = new List<Item>
        {
            new() { Code = "BB-RS-001", Name = "Biji Plastik PP Grade A", CategoryId = catBB.Id, UnitId = unitKg.Id, IsActive = true },
            new() { Code = "BB-PE-004", Name = "Plastik PE Lembaran 0.5mm", CategoryId = catBB.Id, UnitId = unitRoll.Id, IsActive = true },
            new() { Code = "KM-BX-012", Name = "Karton Box 40x30x20cm", CategoryId = catKemasan.Id, UnitId = unitPcs.Id, IsActive = true },
            new() { Code = "BJ-TG-001", Name = "Tangki Plastik 500L Biru", CategoryId = catBJ.Id, UnitId = unitPcs.Id, IsActive = true },
            new() { Code = "SC-BL-003", Name = "Baut Baja M8x40 (Pack)", CategoryId = catSC.Id, UnitId = unitBox.Id, IsActive = true },
            new() { Code = "BB-PG-002", Name = "Pigmen Pewarna Biru", CategoryId = catBB.Id, UnitId = unitKg.Id, IsActive = true },
            new() { Code = "KM-ST-005", Name = "Stiker Label Garansi", CategoryId = catKemasan.Id, UnitId = unitRoll.Id, IsActive = true },
            new() { Code = "SC-SL-008", Name = "Seal Karet EPDM 2 inch", CategoryId = catSC.Id, UnitId = unitPcs.Id, IsActive = false },
        };
        context.Items.AddRange(items);
        await context.SaveChangesAsync();

        // 5. Stock Levels per warehouse (FR-VAL-01: stok minimum per gudang)
        var stockLevels = new List<StockLevel>
        {
            new() { ItemId = items[0].Id, WarehouseId = whUtama.Id, Quantity = 450, MinStock = 100 },
            new() { ItemId = items[0].Id, WarehouseId = whBB.Id, Quantity = 1200, MinStock = 200 },
            new() { ItemId = items[1].Id, WarehouseId = whUtama.Id, Quantity = 18, MinStock = 20 }, // Menipis
            new() { ItemId = items[2].Id, WarehouseId = whUtama.Id, Quantity = 820, MinStock = 150 },
            new() { ItemId = items[3].Id, WarehouseId = whUtama.Id, Quantity = 3, MinStock = 10 }, // Kritis
            new() { ItemId = items[4].Id, WarehouseId = whUtama.Id, Quantity = 45, MinStock = 20 },
            new() { ItemId = items[5].Id, WarehouseId = whBB.Id, Quantity = 8, MinStock = 15 }, // Kritis
            new() { ItemId = items[6].Id, WarehouseId = whUtama.Id, Quantity = 95, MinStock = 50 },
            new() { ItemId = items[7].Id, WarehouseId = whSC.Id, Quantity = 0, MinStock = 25 },
        };
        context.StockLevels.AddRange(stockLevels);
        await context.SaveChangesAsync();

        // 6. Alerts awal untuk barang di bawah stok minimum
        context.Alerts.AddRange(
            new Alert { ItemId = items[3].Id, WarehouseId = whUtama.Id, Level = "kritis", Message = "Stok Tangki Plastik 500L Biru kritis (3 / min 10 Pcs)", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new Alert { ItemId = items[5].Id, WarehouseId = whBB.Id, Level = "kritis", Message = "Stok Pigmen Pewarna Biru kritis (8 / min 15 Kg)", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new Alert { ItemId = items[1].Id, WarehouseId = whUtama.Id, Level = "menipis", Message = "Stok Plastik PE Lembaran 0.5mm menipis (18 / min 20 Roll)", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-6) },
            new Alert { ItemId = items[2].Id, WarehouseId = whUtama.Id, Level = "menipis", Message = "Stok Karton Box mendekati batas aman", IsRead = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );

        // 7. Initial Transactions & Audit Logs
        var tx1 = new StockTransaction
        {
            TransactionNo = "TRX-260923-001",
            Type = TransactionType.IN,
            WarehouseId = whUtama.Id,
            TransactionDate = DateTime.UtcNow.Date.AddDays(-1),
            ReferenceNo = "SJ-SUP-2026-0089",
            Notes = "Pengiriman rutin dari Supplier Utama",
            CreatedBy = "Budi Santoso",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Lines =
            [
                new() { ItemId = items[0].Id, Quantity = 200 },
                new() { ItemId = items[2].Id, Quantity = 300 },
            ],
        };

        var tx2 = new StockTransaction
        {
            TransactionNo = "TRX-260924-002",
            Type = TransactionType.OUT,
            WarehouseId = whUtama.Id,
            TransactionDate = DateTime.UtcNow.Date,
            ReferenceNo = "WO-PRD-2026-0145",
            Notes = "Pengeluaran untuk produksi batch 145",
            CreatedBy = "Sari Wulandari",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            Lines =
            [
                new() { ItemId = items[1].Id, Quantity = 5 },
            ],
        };

        context.StockTransactions.AddRange(tx1, tx2);

        context.AuditLogs.AddRange(
            new AuditLog { UserId = admin.Id, Username = "admin", UserFullName = "Budi Santoso", Action = "Membuat", EntityName = "Item", EntityId = items[0].Id.ToString(), Detail = "Menambahkan barang baru: Biji Plastik PP Grade A", IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new AuditLog { UserId = admin.Id, Username = "admin", UserFullName = "Budi Santoso", Action = "Membuat", EntityName = "Transaction", EntityId = tx1.TransactionNo, Detail = "Mencatat barang masuk nomor " + tx1.TransactionNo, IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new AuditLog { UserId = staf.Id, Username = "sari", UserFullName = "Sari Wulandari", Action = "Membuat", EntityName = "Transaction", EntityId = tx2.TransactionNo, Detail = "Mencatat barang keluar nomor " + tx2.TransactionNo, IpAddress = "127.0.0.1", CreatedAt = DateTime.UtcNow.AddHours(-2) }
        );

        await context.SaveChangesAsync();
    }
}