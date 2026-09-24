using GudangPro.Application.DTOs;
using GudangPro.Domain.Entities;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface IInventoryService
{
    Task<List<ItemDto>> GetItemsAsync(string? search = null, string? status = null);
    Task<ItemDto> GetItemByIdAsync(int id);
    Task<ItemDto> CreateItemAsync(CreateItemRequest request, string username, string userFullName, string? ipAddress);
    Task<ItemDto> UpdateItemAsync(int id, UpdateItemRequest request, string username, string userFullName, string? ipAddress);
    Task DeactivateItemAsync(int id, string username, string userFullName, string? ipAddress);

    Task<List<WarehouseDto>> GetWarehousesAsync(string? search = null);
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, string username, string userFullName, string? ipAddress);
    Task<WarehouseDto> UpdateWarehouseAsync(int id, UpdateWarehouseRequest request, string username, string userFullName, string? ipAddress);

    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<UnitDto>> GetUnitsAsync();
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public InventoryService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<ItemDto>> GetItemsAsync(string? search = null, string? status = null)
    {
        var query = _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Include(i => i.StockLevels)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(i => i.Code.ToLower().Contains(q) || i.Name.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "Semua")
        {
            var active = status == "Aktif";
            query = query.Where(i => i.IsActive == active);
        }

        var items = await query
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(i => new ItemDto(
            i.Id,
            i.Code,
            i.Name,
            i.CategoryId,
            i.Category.Name,
            i.UnitId,
            i.Unit.Name,
            i.StockLevels.Count != 0 ? i.StockLevels.Max(s => s.MinStock) : 0,
            i.StockLevels.Sum(s => s.Quantity),
            i.IsActive,
            i.StockLevels.ToDictionary(s => s.WarehouseId, s => s.Quantity)
        )).ToList();
    }

    public async Task<ItemDto> GetItemByIdAsync(int id)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Include(i => i.StockLevels)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
            throw new KeyNotFoundException("Barang tidak ditemukan.");

        return new ItemDto(
            item.Id,
            item.Code,
            item.Name,
            item.CategoryId,
            item.Category.Name,
            item.UnitId,
            item.Unit.Name,
            item.StockLevels.Any() ? item.StockLevels.Max(s => s.MinStock) : 0,
            item.StockLevels.Sum(s => s.Quantity),
            item.IsActive,
            item.StockLevels.ToDictionary(s => s.WarehouseId, s => s.Quantity)
        );
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemRequest request, string username, string userFullName, string? ipAddress)
    {
        // FR-MST-02: Kode barang unik
        if (await _db.Items.AnyAsync(i => i.Code == request.Code))
            throw new InvalidOperationException($"Kode barang '{request.Code}' sudah digunakan.");

        var item = new Item
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            IsActive = true
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        // Buat stock level awal untuk semua gudang aktif
        var warehouses = await _db.Warehouses.Where(w => w.IsActive).ToListAsync();
        foreach (var wh in warehouses)
        {
            var initialQty = (wh.Id == request.InitialWarehouseId) ? (request.InitialQuantity ?? 0) : 0;
            _db.StockLevels.Add(new StockLevel
            {
                ItemId = item.Id,
                WarehouseId = wh.Id,
                Quantity = initialQty,
                MinStock = request.MinStock
            });
        }
        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        await _audit.LogAsync(user?.Id, userFullName, username, "Membuat", "Item", item.Id.ToString(), null, $"Menambahkan barang baru: {item.Name} ({item.Code})", ipAddress);

        return await GetItemByIdAsync(item.Id);
    }

    public async Task<ItemDto> UpdateItemAsync(int id, UpdateItemRequest request, string username, string userFullName, string? ipAddress)
    {
        var item = await _db.Items
            .Include(i => i.StockLevels)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
            throw new KeyNotFoundException("Barang tidak ditemukan.");

        var oldDetail = $"{item.Name}, MinStock: {item.StockLevels.FirstOrDefault()?.MinStock}";
        item.Name = request.Name.Trim();
        item.CategoryId = request.CategoryId;
        item.UnitId = request.UnitId;

        foreach (var sl in item.StockLevels)
        {
            sl.MinStock = request.MinStock;
        }

        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        await _audit.LogAsync(user?.Id, userFullName, username, "Mengubah", "Item", item.Id.ToString(), oldDetail, $"Mengubah barang: {item.Name}", ipAddress);

        return await GetItemByIdAsync(item.Id);
    }

    // FR-MST-05: Sistem tidak menghapus barang yang sudah punya transaksi, hanya menonaktifkan
    public async Task DeactivateItemAsync(int id, string username, string userFullName, string? ipAddress)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id);
        if (item == null)
            throw new KeyNotFoundException("Barang tidak ditemukan.");

        item.IsActive = false;
        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        await _audit.LogAsync(user?.Id, userFullName, username, "Menonaktifkan", "Item", item.Id.ToString(), null, $"Menonaktifkan barang: {item.Name} ({item.Code})", ipAddress);
    }

    public async Task<List<WarehouseDto>> GetWarehousesAsync(string? search = null)
    {
        var query = _db.Warehouses
            .AsNoTracking()
            .Include(w => w.StockLevels)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(w => w.Code.ToLower().Contains(q) || w.Name.ToLower().Contains(q));
        }

        var warehouses = await query
            .OrderBy(w => w.Name)
            .ToListAsync();

        return warehouses.Select(w => new WarehouseDto(
            w.Id,
            w.Code,
            w.Name,
            w.Address,
            w.IsActive,
            w.StockLevels.Count(s => s.Quantity > 0),
            w.StockLevels.Sum(s => s.Quantity)
        )).ToList();
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, string username, string userFullName, string? ipAddress)
    {
        if (await _db.Warehouses.AnyAsync(w => w.Code == request.Code))
            throw new InvalidOperationException($"Kode gudang '{request.Code}' sudah digunakan.");

        var warehouse = new Warehouse
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            Address = request.Address?.Trim(),
            IsActive = true
        };

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        // Buat stock level untuk semua barang yang ada
        var items = await _db.Items.ToListAsync();
        foreach (var it in items)
        {
            _db.StockLevels.Add(new StockLevel
            {
                ItemId = it.Id,
                WarehouseId = warehouse.Id,
                Quantity = 0,
                MinStock = 10
            });
        }
        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        await _audit.LogAsync(user?.Id, userFullName, username, "Membuat", "Warehouse", warehouse.Id.ToString(), null, $"Menambahkan gudang baru: {warehouse.Name} ({warehouse.Code})", ipAddress);

        return new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Address, true, 0, 0);
    }

    public async Task<WarehouseDto> UpdateWarehouseAsync(int id, UpdateWarehouseRequest request, string username, string userFullName, string? ipAddress)
    {
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id);
        if (warehouse == null)
            throw new KeyNotFoundException("Gudang tidak ditemukan.");

        warehouse.Name = request.Name.Trim();
        warehouse.Address = request.Address?.Trim();
        warehouse.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        await _audit.LogAsync(user?.Id, userFullName, username, "Mengubah", "Warehouse", warehouse.Id.ToString(), null, $"Mengubah gudang: {warehouse.Name}", ipAddress);

        var itemCount = await _db.StockLevels.CountAsync(s => s.WarehouseId == id && s.Quantity > 0);
        var totalStock = await _db.StockLevels.Where(s => s.WarehouseId == id).SumAsync(s => s.Quantity);

        return new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Address, warehouse.IsActive, itemCount, totalStock);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();
    }

    public async Task<List<UnitDto>> GetUnitsAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UnitDto(u.Id, u.Name))
            .ToListAsync();
    }
}