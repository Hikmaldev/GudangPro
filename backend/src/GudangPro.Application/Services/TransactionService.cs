using GudangPro.Application.DTOs;
using GudangPro.Domain.Entities;
using GudangPro.Domain.Enums;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface ITransactionService
{
    Task<StockTransactionDto> CreateInboundAsync(CreateInboundTransactionRequest request, string username, string userFullName, string? ipAddress);
    Task<StockTransactionDto> CreateOutboundAsync(CreateOutboundTransactionRequest request, string username, string userFullName, string? ipAddress);
    Task<StockTransactionDto> CancelTransactionAsync(int id, CancelTransactionRequest request, string username, string userFullName, string? ipAddress);
    Task<PagedResult<StockTransactionDto>> GetTransactionsAsync(TransactionFilter filter);
    Task<StockTransactionDto> GetTransactionByIdAsync(int id);
}

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IAlertService _alert;

    public TransactionService(AppDbContext db, IAuditService audit, IAlertService alert)
    {
        _db = db;
        _audit = audit;
        _alert = alert;
    }

    // FR-IN-01..04: Pencatatan barang masuk
    public async Task<StockTransactionDto> CreateInboundAsync(CreateInboundTransactionRequest request, string username, string userFullName, string? ipAddress)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            throw new ArgumentException("Daftar barang tidak boleh kosong.");

        // FR-IN-03: Sistem menolak jumlah nol atau negatif
        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException("Jumlah barang harus lebih besar dari nol.");
        }

        // §9.3: Setiap transaksi berjalan dalam satu database transaction
        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var warehouse = await _db.Warehouses.FindAsync(request.WarehouseId)
                ?? throw new KeyNotFoundException("Gudang tujuan tidak ditemukan.");

            var txNo = await GenerateTransactionNoAsync(TransactionType.IN);

            var tx = new StockTransaction
            {
                TransactionNo = txNo,
                Type = TransactionType.IN,
                WarehouseId = request.WarehouseId,
                TransactionDate = request.TransactionDate,
                ReferenceNo = request.ReferenceNo.Trim(),
                Notes = request.Notes?.Trim(),
                CreatedBy = userFullName,
                CreatedAt = DateTime.UtcNow,
                Lines = []
            };

            foreach (var line in request.Lines)
            {
                var item = await _db.Items.FindAsync(line.ItemId)
                    ?? throw new KeyNotFoundException($"Barang dengan ID {line.ItemId} tidak ditemukan.");

                if (!item.IsActive)
                    throw new InvalidOperationException($"Barang '{item.Name}' tidak aktif dan tidak dapat ditransaksikan.");

                // FR-IN-02: Sistem menambah stok gudang tujuan
                var stock = await _db.StockLevels
                    .FirstOrDefaultAsync(s => s.ItemId == line.ItemId && s.WarehouseId == request.WarehouseId);

                if (stock == null)
                {
                    stock = new StockLevel
                    {
                        ItemId = line.ItemId,
                        WarehouseId = request.WarehouseId,
                        Quantity = line.Quantity,
                        MinStock = 10
                    };
                    _db.StockLevels.Add(stock);
                }
                else
                {
                    stock.Quantity += line.Quantity;
                }

                tx.Lines.Add(new StockTransactionLine
                {
                    ItemId = line.ItemId,
                    Quantity = line.Quantity
                });
            }

            _db.StockTransactions.Add(tx);
            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            await _audit.LogAsync(user?.Id, userFullName, username, "Membuat", "Transaction", tx.TransactionNo, null, $"Barang masuk: {tx.TransactionNo} ({tx.Lines.Count} baris)", ipAddress);

            return await GetTransactionByIdAsync(tx.Id);
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // FR-OUT-01..03: Pencatatan barang keluar + validasi stok
    public async Task<StockTransactionDto> CreateOutboundAsync(CreateOutboundTransactionRequest request, string username, string userFullName, string? ipAddress)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            throw new ArgumentException("Daftar barang tidak boleh kosong.");

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException("Jumlah barang harus lebih besar dari nol.");
        }

        // §9.3: Menggunakan database transaction dan concurrency control
        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var warehouse = await _db.Warehouses.FindAsync(request.WarehouseId)
                ?? throw new KeyNotFoundException("Gudang asal tidak ditemukan.");

            var txNo = await GenerateTransactionNoAsync(TransactionType.OUT);

            var tx = new StockTransaction
            {
                TransactionNo = txNo,
                Type = TransactionType.OUT,
                WarehouseId = request.WarehouseId,
                TransactionDate = request.TransactionDate,
                ReferenceNo = request.ReferenceNo.Trim(),
                Notes = request.Notes?.Trim(),
                CreatedBy = userFullName,
                CreatedAt = DateTime.UtcNow,
                Lines = []
            };

            var alertsToCreate = new List<(Item Item, int RemainingQty, int MinStock)>();

            foreach (var line in request.Lines)
            {
                var item = await _db.Items.FindAsync(line.ItemId)
                    ?? throw new KeyNotFoundException($"Barang dengan ID {line.ItemId} tidak ditemukan.");

                if (!item.IsActive)
                    throw new InvalidOperationException($"Barang '{item.Name}' tidak aktif.");

                var stock = await _db.StockLevels
                    .FirstOrDefaultAsync(s => s.ItemId == line.ItemId && s.WarehouseId == request.WarehouseId);

                var available = stock?.Quantity ?? 0;

                // FR-OUT-02 / Aturan Bisnis #2: Tolak jika melebihi stok tersedia (stok tidak boleh negatif)
                if (line.Quantity > available)
                {
                    throw new InvalidOperationException(
                        $"Stok '{item.Name}' di {warehouse.Name} tidak cukup. Tersedia: {available}, diminta: {line.Quantity}.");
                }

                stock!.Quantity -= line.Quantity;

                // FR-OUT-03 / Aturan Bisnis #4: Alert muncul saat Quantity <= MinStock setelah transaksi keluar
                if (stock.Quantity <= stock.MinStock)
                {
                    alertsToCreate.Add((item, stock.Quantity, stock.MinStock));
                }

                tx.Lines.Add(new StockTransactionLine
                {
                    ItemId = line.ItemId,
                    Quantity = line.Quantity
                });
            }

            _db.StockTransactions.Add(tx);
            await _db.SaveChangesAsync();

            // Buat alert otomatis jika ada yang di bawah stok minimum
            foreach (var a in alertsToCreate)
            {
                var level = a.RemainingQty <= (a.MinStock / 2) ? "kritis" : "menipis";
                var msg = $"Stok {a.Item.Name} di {warehouse.Name} {level} ({a.RemainingQty} / min {a.MinStock})";
                await _alert.CreateAlertAsync(a.Item.Id, warehouse.Id, msg, level);
            }

            await dbTx.CommitAsync();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            await _audit.LogAsync(user?.Id, userFullName, username, "Membuat", "Transaction", tx.TransactionNo, null, $"Barang keluar: {tx.TransactionNo} ({tx.Lines.Count} baris)", ipAddress);

            return await GetTransactionByIdAsync(tx.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            await dbTx.RollbackAsync();
            throw new InvalidOperationException("Terjadi konflik transaksi: data stok telah diubah oleh pengguna lain. Silakan periksa saldo terkini dan coba lagi.");
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // FR-IN-05 & FR-OUT-05 & Aturan Bisnis #3: Pembatalan via transaksi koreksi (tidak dihapus fisik)
    public async Task<StockTransactionDto> CancelTransactionAsync(int id, CancelTransactionRequest request, string username, string userFullName, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Alasan pembatalan harus diisi.");

        await using var dbTx = await _db.Database.BeginTransactionAsync();
        try
        {
            var original = await _db.StockTransactions
                .Include(t => t.Lines)
                .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (original == null)
                throw new KeyNotFoundException("Transaksi tidak ditemukan.");

            if (original.IsCancelled)
                throw new InvalidOperationException("Transaksi ini sudah pernah dibatalkan.");

            // Balikkan efek transaksi:
            // Jika asalnya IN -> kurangi stok (cek jangan sampai negatif)
            // Jika asalnya OUT -> kembalikan/tambah stok
            var koreksiLines = new List<StockTransactionLine>();

            foreach (var line in original.Lines)
            {
                var stock = await _db.StockLevels
                    .FirstOrDefaultAsync(s => s.ItemId == line.ItemId && s.WarehouseId == original.WarehouseId);

                if (original.Type == TransactionType.IN)
                {
                    if (stock == null || stock.Quantity < line.Quantity)
                        throw new InvalidOperationException($"Tidak dapat membatalkan: stok '{line.Item.Name}' saat ini ({stock?.Quantity ?? 0}) lebih kecil dari jumlah yang akan dikurangi ({line.Quantity}).");

                    stock.Quantity -= line.Quantity;
                }
                else if (original.Type == TransactionType.OUT)
                {
                    if (stock == null)
                    {
                        stock = new StockLevel { ItemId = line.ItemId, WarehouseId = original.WarehouseId, Quantity = line.Quantity, MinStock = 10 };
                        _db.StockLevels.Add(stock);
                    }
                    else
                    {
                        stock.Quantity += line.Quantity;
                    }
                }

                koreksiLines.Add(new StockTransactionLine
                {
                    ItemId = line.ItemId,
                    Quantity = line.Quantity
                });
            }

            original.IsCancelled = true;

            // Buat transaksi koreksi (Type = ADJUST)
            var koreksiTx = new StockTransaction
            {
                TransactionNo = await GenerateTransactionNoAsync(TransactionType.ADJUST),
                Type = TransactionType.ADJUST,
                WarehouseId = original.WarehouseId,
                TransactionDate = DateTime.UtcNow.Date,
                ReferenceNo = $"BATAL-{original.TransactionNo}",
                Notes = $"Koreksi pembatalan transaksi {original.TransactionNo}: {request.Reason}",
                CreatedBy = userFullName,
                CreatedAt = DateTime.UtcNow,
                CancelledOfId = original.Id,
                CancellationReason = request.Reason,
                Lines = koreksiLines
            };

            _db.StockTransactions.Add(koreksiTx);
            await _db.SaveChangesAsync();
            await dbTx.CommitAsync();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            await _audit.LogAsync(user?.Id, userFullName, username, "Membatalkan", "Transaction", original.TransactionNo, null, $"Membatalkan {original.TransactionNo} dengan alasan: {request.Reason}. Transaksi koreksi: {koreksiTx.TransactionNo}", ipAddress);

            return await GetTransactionByIdAsync(koreksiTx.Id);
        }
        catch
        {
            await dbTx.RollbackAsync();
            throw;
        }
    }

    // FR-HIS-01..03: Riwayat transaksi dengan filter lengkap & pagination
    public async Task<PagedResult<StockTransactionDto>> GetTransactionsAsync(TransactionFilter filter)
    {
        var query = _db.StockTransactions
            .AsNoTracking()
            .Include(t => t.Warehouse)
            .Include(t => t.Lines)
            .ThenInclude(l => l.Item)
            .ThenInclude(i => i.Unit)
            .AsQueryable();

        if (filter.WarehouseId.HasValue && filter.WarehouseId.Value > 0)
            query = query.Where(t => t.WarehouseId == filter.WarehouseId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value.Date);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value.Date.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrWhiteSpace(filter.Type) && filter.Type != "Semua")
        {
            if (Enum.TryParse<TransactionType>(filter.Type, true, out var tType))
                query = query.Where(t => t.Type == tType);
        }

        if (filter.ItemId.HasValue)
            query = query.Where(t => t.Lines.Any(l => l.ItemId == filter.ItemId.Value));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var q = filter.Search.Trim().ToLower();
            query = query.Where(t => t.TransactionNo.ToLower().Contains(q) ||
                                     t.ReferenceNo.ToLower().Contains(q) ||
                                     t.CreatedBy.ToLower().Contains(q) ||
                                     t.Lines.Any(l => l.Item.Name.ToLower().Contains(q) || l.Item.Code.ToLower().Contains(q)));
        }

        var total = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = transactions.Select(t => new StockTransactionDto(
            t.Id,
            t.TransactionNo,
            t.Type.ToString(),
            t.WarehouseId,
            t.Warehouse.Name,
            t.TransactionDate,
            t.ReferenceNo,
            t.Notes,
            t.CreatedBy,
            t.CreatedAt,
            t.IsCancelled ? "Dibatalkan" : "Berhasil",
            t.CancelledOfId,
            t.CancellationReason,
            t.Lines.Select(l => new TransactionLineDto(
                l.ItemId,
                l.Item.Code,
                l.Item.Name,
                l.Quantity,
                l.Item.Unit.Name,
                0
            )).ToList()
        )).ToList();

        return new PagedResult<StockTransactionDto>(items, total, page, pageSize, totalPages);
    }

    public async Task<StockTransactionDto> GetTransactionByIdAsync(int id)
    {
        var tx = await _db.StockTransactions
            .AsNoTracking()
            .Include(t => t.Warehouse)
            .Include(t => t.Lines)
            .ThenInclude(l => l.Item)
            .ThenInclude(i => i.Unit)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tx == null)
            throw new KeyNotFoundException("Transaksi tidak ditemukan.");

        return new StockTransactionDto(
            tx.Id,
            tx.TransactionNo,
            tx.Type.ToString(),
            tx.WarehouseId,
            tx.Warehouse.Name,
            tx.TransactionDate,
            tx.ReferenceNo,
            tx.Notes,
            tx.CreatedBy,
            tx.CreatedAt,
            tx.IsCancelled ? "Dibatalkan" : "Berhasil",
            tx.CancelledOfId,
            tx.CancellationReason,
            tx.Lines.Select(l => new TransactionLineDto(
                l.ItemId,
                l.Item.Code,
                l.Item.Name,
                l.Quantity,
                l.Item.Unit.Name,
                0
            )).ToList()
        );
    }

    private async Task<string> GenerateTransactionNoAsync(TransactionType type)
    {
        var prefix = type switch
        {
            TransactionType.IN => "TRX-IN",
            TransactionType.OUT => "TRX-OUT",
            TransactionType.ADJUST => "TRX-ADJ",
            _ => "TRX"
        };

        var datePart = DateTime.UtcNow.ToString("yyMMdd");
        var countToday = await _db.StockTransactions
            .CountAsync(t => t.CreatedAt.Date == DateTime.UtcNow.Date);

        return $"{prefix}-{datePart}-{(countToday + 1):D3}";
    }
}