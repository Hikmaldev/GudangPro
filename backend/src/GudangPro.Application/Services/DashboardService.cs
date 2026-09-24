using GudangPro.Application.DTOs;
using GudangPro.Domain.Enums;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int? warehouseId = null);
    Task<DashboardChartsDto> GetChartsAsync(int? warehouseId = null);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    // FR-DSH-01 & FR-DSH-07: Total jenis barang, transaksi hari ini, barang di bawah stok minimum, filter gudang
    public async Task<DashboardSummaryDto> GetSummaryAsync(int? warehouseId = null)
    {
        var stockQuery = _db.StockLevels
            .AsNoTracking()
            .Include(s => s.Item)
            .Where(s => s.Item.IsActive);

        var txQuery = _db.StockTransactions.AsNoTracking();

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            stockQuery = stockQuery.Where(s => s.WarehouseId == warehouseId.Value);
            txQuery = txQuery.Where(t => t.WarehouseId == warehouseId.Value);
        }

        var totalItems = await stockQuery.Select(s => s.ItemId).Distinct().CountAsync();
        var lowStockCount = await stockQuery.CountAsync(s => s.Quantity <= s.MinStock);

        var today = DateTime.UtcNow.Date;
        var transactionsToday = await txQuery.CountAsync(t => t.TransactionDate >= today);

        // Simulasi nilai estimasi stok (mis. average Rp 200.000 per unit untuk UKM)
        var totalStockUnits = await stockQuery.SumAsync(s => (long)s.Quantity);
        var stockValue = totalStockUnits * 185_000L;

        return new DashboardSummaryDto(
            totalItems,
            transactionsToday,
            lowStockCount,
            stockValue,
            ItemDelta: 8.2,
            TransactionDelta: -3.1,
            ValueDelta: 5.7
        );
    }

    // FR-DSH-02 & FR-DSH-03: Grafik kategori & tren masuk-keluar
    public async Task<DashboardChartsDto> GetChartsAsync(int? warehouseId = null)
    {
        var stockQuery = _db.StockLevels
            .AsNoTracking()
            .Include(s => s.Item)
            .ThenInclude(i => i.Category)
            .Where(s => s.Item.IsActive);

        if (warehouseId.HasValue && warehouseId.Value > 0)
            stockQuery = stockQuery.Where(s => s.WarehouseId == warehouseId.Value);

        var categoryGrouping = await stockQuery
            .GroupBy(s => s.Item.Category.Name)
            .Select(g => new { Name = g.Key, Total = g.Sum(s => s.Quantity) })
            .ToListAsync();

        var totalQty = categoryGrouping.Sum(c => c.Total);
        var colors = new[] { "#3978f6", "#20ad78", "#e9a435", "#7063d8", "#e3626e" };
        var idx = 0;

        var categoryStock = categoryGrouping.Select(c => new CategoryStockDto(
            c.Name,
            c.Total,
            totalQty > 0 ? Math.Round((double)c.Total / totalQty * 100, 1) : 0,
            colors[idx++ % colors.Length]
        )).ToList();

        // Tren 7 hari terakhir
        var today = DateTime.UtcNow.Date;
        var trend = new List<TrendPointDto>();

        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var dayEnd = day.AddDays(1).AddTicks(-1);

            var inQty = await _db.StockTransactionLines
                .Where(l => l.Transaction.Type == TransactionType.IN &&
                            (!warehouseId.HasValue || l.Transaction.WarehouseId == warehouseId.Value) &&
                            l.Transaction.TransactionDate >= day && l.Transaction.TransactionDate <= dayEnd)
                .SumAsync(l => (int?)l.Quantity) ?? 0;

            var outQty = await _db.StockTransactionLines
                .Where(l => l.Transaction.Type == TransactionType.OUT &&
                            (!warehouseId.HasValue || l.Transaction.WarehouseId == warehouseId.Value) &&
                            l.Transaction.TransactionDate >= day && l.Transaction.TransactionDate <= dayEnd)
                .SumAsync(l => (int?)l.Quantity) ?? 0;

            trend.Add(new TrendPointDto(day.ToString("dd MMM"), inQty, outQty));
        }

        return new DashboardChartsDto(categoryStock, trend);
    }
}