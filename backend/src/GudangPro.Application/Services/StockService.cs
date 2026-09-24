using GudangPro.Application.DTOs;
using GudangPro.Domain.Enums;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface IStockService
{
    Task<List<StockLevelDto>> GetStockLevelsAsync(int? warehouseId = null, string? search = null);
    Task<StockCardDto> GetStockCardAsync(int itemId, int warehouseId, DateTime? startDate = null, DateTime? endDate = null);
}

public class StockService : IStockService
{
    private readonly AppDbContext _db;

    public StockService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<StockLevelDto>> GetStockLevelsAsync(int? warehouseId = null, string? search = null)
    {
        var query = _db.StockLevels
            .AsNoTracking()
            .Include(s => s.Item)
            .Include(s => s.Warehouse)
            .Where(s => s.Item.IsActive && s.Warehouse.IsActive)
            .AsQueryable();

        if (warehouseId.HasValue && warehouseId.Value > 0)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(s => s.Item.Name.ToLower().Contains(q) || s.Item.Code.ToLower().Contains(q));
        }

        var levels = await query
            .OrderBy(s => s.Item.Name)
            .ToListAsync();

        return levels.Select(s => new StockLevelDto(
            s.ItemId,
            s.Item.Code,
            s.Item.Name,
            s.WarehouseId,
            s.Warehouse.Name,
            s.Quantity,
            s.MinStock,
            s.Quantity <= (s.MinStock / 2) ? "Kritis" : (s.Quantity <= s.MinStock ? "Menipis" : "Aman")
        )).ToList();
    }

    // FR-HIS-05: Kartu stok per barang (saldo awal, masuk, keluar, saldo akhir)
    public async Task<StockCardDto> GetStockCardAsync(int itemId, int warehouseId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Include(i => i.StockLevels)
            .FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new KeyNotFoundException("Barang tidak ditemukan.");

        var warehouse = await _db.Warehouses.FindAsync(warehouseId)
            ?? throw new KeyNotFoundException("Gudang tidak ditemukan.");

        var currentStock = item.StockLevels.FirstOrDefault(s => s.WarehouseId == warehouseId)?.Quantity ?? 0;

        // Ambil semua transaksi barang ini di gudang ini, diurutkan kronologis
        var transactions = await _db.StockTransactions
            .AsNoTracking()
            .Include(t => t.Lines)
            .Where(t => t.WarehouseId == warehouseId && !t.IsCancelled && t.Lines.Any(l => l.ItemId == itemId))
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        var start = startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = endDate?.Date ?? DateTime.UtcNow.Date;

        // Hitung mutasi kumulatif untuk mendapatkan saldo awal sebelum rentang tanggal
        var runningBalance = 0;
        var openingBalance = 0;
        var totalIn = 0;
        var totalOut = 0;
        var entries = new List<StockCardEntryDto>();

        foreach (var tx in transactions)
        {
            var line = tx.Lines.First(l => l.ItemId == itemId);
            int inQty = 0;
            int outQty = 0;

            if (tx.Type == TransactionType.IN)
            {
                inQty = line.Quantity;
                runningBalance += inQty;
            }
            else if (tx.Type == TransactionType.OUT)
            {
                outQty = line.Quantity;
                runningBalance -= outQty;
            }
            else if (tx.Type == TransactionType.ADJUST)
            {
                // Transaksi koreksi/pembatalan
                if (tx.ReferenceNo.StartsWith("BATAL-TRX-IN"))
                {
                    outQty = line.Quantity;
                    runningBalance -= outQty;
                }
                else
                {
                    inQty = line.Quantity;
                    runningBalance += inQty;
                }
            }

            if (tx.TransactionDate.Date < start)
            {
                openingBalance = runningBalance;
            }
            else if (tx.TransactionDate.Date <= end)
            {
                totalIn += inQty;
                totalOut += outQty;

                entries.Add(new StockCardEntryDto(
                    tx.TransactionDate,
                    tx.TransactionNo,
                    tx.ReferenceNo,
                    inQty > 0 ? inQty : null,
                    outQty > 0 ? outQty : null,
                    runningBalance,
                    tx.CreatedBy
                ));
            }
        }

        var itemDto = new ItemDto(
            item.Id,
            item.Code,
            item.Name,
            item.CategoryId,
            item.Category.Name,
            item.UnitId,
            item.Unit.Name,
            item.StockLevels.FirstOrDefault(s => s.WarehouseId == warehouseId)?.MinStock ?? 0,
            currentStock,
            item.IsActive,
            item.StockLevels.ToDictionary(s => s.WarehouseId, s => s.Quantity)
        );

        return new StockCardDto(
            itemDto,
            warehouse.Id,
            warehouse.Name,
            openingBalance,
            totalIn,
            totalOut,
            runningBalance,
            entries
        );
    }
}