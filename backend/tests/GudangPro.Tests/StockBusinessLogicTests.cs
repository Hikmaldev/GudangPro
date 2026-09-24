using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using GudangPro.Domain.Entities;
using GudangPro.Domain.Enums;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GudangPro.Tests;

public class StockBusinessLogicTests
{
    private AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(options);
        return db;
    }

    private async Task<(AppDbContext db, Warehouse wh, Item item)> SeedBasicDataAsync(string dbName, int initialStock = 50, int minStock = 10)
    {
        var db = CreateDbContext(dbName);
        var cat = new Category { Name = "Bahan Baku" };
        var unit = new Unit { Name = "Pcs" };
        db.Categories.Add(cat);
        db.Units.Add(unit);
        await db.SaveChangesAsync();

        var wh = new Warehouse { Code = "GDG-TEST", Name = "Gudang Test", IsActive = true };
        db.Warehouses.Add(wh);

        var item = new Item { Code = "TEST-001", Name = "Barang Uji", CategoryId = cat.Id, UnitId = unit.Id, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var sl = new StockLevel { ItemId = item.Id, WarehouseId = wh.Id, Quantity = initialStock, MinStock = minStock };
        db.StockLevels.Add(sl);
        await db.SaveChangesAsync();

        return (db, wh, item);
    }

    // FR-IN-02: Sistem menambah stok gudang tujuan saat transaksi disimpan
    [Fact]
    public async Task Inbound_IncreasesStock_Successfully()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Inbound_IncreasesStock_Successfully), initialStock: 50);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        var req = new CreateInboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "SJ-001",
            Notes: "Uji Inbound",
            Lines: [new TransactionLineRequest(item.Id, 25)]
        );

        var result = await txService.CreateInboundAsync(req, "admin", "Budi Santoso", "127.0.0.1");

        Assert.NotNull(result);
        Assert.Equal("IN", result.Type);

        var updatedStock = await db.StockLevels.FirstAsync(s => s.ItemId == item.Id && s.WarehouseId == wh.Id);
        Assert.Equal(75, updatedStock.Quantity);
    }

    // FR-IN-03: Sistem menolak jumlah nol atau negatif
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Inbound_RejectsZeroOrNegativeQuantity(int invalidQty)
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Inbound_RejectsZeroOrNegativeQuantity) + invalidQty);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        var req = new CreateInboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "SJ-002",
            Notes: null,
            Lines: [new TransactionLineRequest(item.Id, invalidQty)]
        );

        await Assert.ThrowsAsync<ArgumentException>(() =>
            txService.CreateInboundAsync(req, "admin", "Budi Santoso", null));
    }

    // FR-OUT-01: Pengguna membuat transaksi keluar, stok berkurang
    [Fact]
    public async Task Outbound_DecreasesStock_Successfully()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Outbound_DecreasesStock_Successfully), initialStock: 50, minStock: 10);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        var req = new CreateOutboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "WO-001",
            Notes: null,
            Lines: [new TransactionLineRequest(item.Id, 20)]
        );

        var result = await txService.CreateOutboundAsync(req, "admin", "Budi Santoso", null);

        Assert.NotNull(result);
        Assert.Equal("OUT", result.Type);

        var updatedStock = await db.StockLevels.FirstAsync(s => s.ItemId == item.Id && s.WarehouseId == wh.Id);
        Assert.Equal(30, updatedStock.Quantity);
    }

    // FR-OUT-02 / Aturan Bisnis #2: Sistem menolak transaksi jika melebihi stok tersedia (stok tidak boleh negatif)
    [Fact]
    public async Task Outbound_Rejects_WhenQuantityExceedsAvailableStock()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Outbound_Rejects_WhenQuantityExceedsAvailableStock), initialStock: 10);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        var req = new CreateOutboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "WO-OVER",
            Notes: null,
            Lines: [new TransactionLineRequest(item.Id, 15)] // Minta 15 padahal stok cuma 10
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            txService.CreateOutboundAsync(req, "admin", "Budi Santoso", null));

        Assert.Contains("tidak cukup", ex.Message);

        // Pastikan stok tidak berubah sama sekali
        var stock = await db.StockLevels.FirstAsync(s => s.ItemId == item.Id && s.WarehouseId == wh.Id);
        Assert.Equal(10, stock.Quantity);
    }

    // FR-OUT-03 / Aturan Bisnis #4: Sistem membuat alert saat stok jatuh sampai atau di bawah stok minimum
    [Fact]
    public async Task Outbound_CreatesAlert_WhenStockDropsBelowMinimum()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Outbound_CreatesAlert_WhenStockDropsBelowMinimum), initialStock: 15, minStock: 10);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        // Keluarkan 11 unit -> sisa 4 (di bawah min/2 = 5 -> kritis)
        var req = new CreateOutboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "WO-ALERT",
            Notes: null,
            Lines: [new TransactionLineRequest(item.Id, 11)]
        );

        await txService.CreateOutboundAsync(req, "admin", "Budi Santoso", null);

        var alerts = await db.Alerts.Where(a => a.ItemId == item.Id && a.WarehouseId == wh.Id).ToListAsync();
        Assert.NotEmpty(alerts);
        Assert.Contains("kritis", alerts.First().Level);
    }

    // FR-IN-05, FR-OUT-05, Aturan Bisnis #3: Pembatalan via transaksi koreksi (tidak menghapus transaksi asli)
    [Fact]
    public async Task Cancel_CreatesCorrectionTransaction_AndReversesStock()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(Cancel_CreatesCorrectionTransaction_AndReversesStock), initialStock: 50);
        var audit = new AuditService(db);
        var alert = new AlertService(db);
        var txService = new TransactionService(db, audit, alert);

        // Buat transaksi keluar 20 -> stok jadi 30
        var outReq = new CreateOutboundTransactionRequest(
            WarehouseId: wh.Id,
            TransactionDate: DateTime.UtcNow,
            ReferenceNo: "WO-TO-CANCEL",
            Notes: null,
            Lines: [new TransactionLineRequest(item.Id, 20)]
        );
        var outTx = await txService.CreateOutboundAsync(outReq, "admin", "Budi Santoso", null);

        var stockAfterOut = await db.StockLevels.FirstAsync(s => s.ItemId == item.Id && s.WarehouseId == wh.Id);
        Assert.Equal(30, stockAfterOut.Quantity);

        // Batalkan transaksi
        var cancelReq = new CancelTransactionRequest(Reason: "Salah input nomor referensi");
        var koreksi = await txService.CancelTransactionAsync(outTx.Id, cancelReq, "admin", "Budi Santoso", null);

        // Verifikasi transaksi koreksi
        Assert.NotNull(koreksi);
        Assert.Equal("ADJUST", koreksi.Type);
        Assert.Equal(outTx.Id, koreksi.CancelledOfId);

        // Transaksi asli ditandai dibatalkan
        var original = await db.StockTransactions.FindAsync(outTx.Id);
        Assert.True(original!.IsCancelled);

        // Saldo stok kembali utuh ke 50
        var stockAfterCancel = await db.StockLevels.FirstAsync(s => s.ItemId == item.Id && s.WarehouseId == wh.Id);
        Assert.Equal(50, stockAfterCancel.Quantity);
    }

    // FR-MST-02: Kode barang bersifat unik
    [Fact]
    public async Task CreateItem_RejectsDuplicateCode()
    {
        var (db, wh, item) = await SeedBasicDataAsync(nameof(CreateItem_RejectsDuplicateCode));
        var audit = new AuditService(db);
        var invService = new InventoryService(db, audit);

        var dupReq = new CreateItemRequest(
            Code: "TEST-001", // Duplikat
            Name: "Barang Lain",
            CategoryId: item.CategoryId,
            UnitId: item.UnitId,
            MinStock: 5
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            invService.CreateItemAsync(dupReq, "admin", "Budi Santoso", null));

        Assert.Contains("sudah digunakan", ex.Message);
    }
}