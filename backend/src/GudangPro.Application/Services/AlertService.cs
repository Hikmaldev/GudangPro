using GudangPro.Application.DTOs;
using GudangPro.Domain.Entities;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface IAlertService
{
    Task<List<AlertDto>> GetAlertsAsync(string? filter = null);
    Task CreateAlertAsync(int itemId, int warehouseId, string message, string level);
    Task MarkReadAsync(int id);
    Task MarkAllReadAsync();
}

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;

    public AlertService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AlertDto>> GetAlertsAsync(string? filter = null)
    {
        var query = _db.Alerts
            .AsNoTracking()
            .Include(a => a.Item)
            .Include(a => a.Warehouse)
            .AsQueryable();

        if (filter == "Belum dibaca")
            query = query.Where(a => !a.IsRead);
        else if (filter == "kritis")
            query = query.Where(a => a.Level == "kritis");
        else if (filter == "menipis")
            query = query.Where(a => a.Level == "menipis");

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AlertDto(
                a.Id,
                a.ItemId,
                a.Item.Code,
                a.Item.Name,
                a.WarehouseId,
                a.Warehouse.Name,
                a.Message,
                a.Level,
                a.IsRead,
                a.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task CreateAlertAsync(int itemId, int warehouseId, string message, string level)
    {
        // Hindari spam alert yang identik dalam 1 jam terakhir
        var recentExists = await _db.Alerts.AnyAsync(a =>
            a.ItemId == itemId &&
            a.WarehouseId == warehouseId &&
            !a.IsRead &&
            a.CreatedAt >= DateTime.UtcNow.AddHours(-1));

        if (recentExists) return;

        var alert = new Alert
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            Message = message,
            Level = level,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync();
    }

    public async Task MarkReadAsync(int id)
    {
        var alert = await _db.Alerts.FindAsync(id);
        if (alert != null)
        {
            alert.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkAllReadAsync()
    {
        var unread = await _db.Alerts.Where(a => !a.IsRead).ToListAsync();
        foreach (var a in unread)
        {
            a.IsRead = true;
        }
        await _db.SaveChangesAsync();
    }
}