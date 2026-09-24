using GudangPro.Application.DTOs;
using GudangPro.Domain.Entities;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GudangPro.Application.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string userFullName, string username, string action, string entityName, string? entityId, string? oldValue, string? detail, string? ipAddress);
    Task<List<AuditLogDto>> GetLogsAsync(string? action = null, string? search = null, int limit = 100);
}

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int? userId, string userFullName, string username, string action, string entityName, string? entityId, string? oldValue, string? detail, string? ipAddress)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserFullName = userFullName,
            Username = username,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = oldValue,
            Detail = detail,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(string? action = null, string? search = null, int limit = 100)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(action) && action != "Semua")
        {
            query = query.Where(l => l.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(l => l.UserFullName.ToLower().Contains(q) ||
                                     l.Username.ToLower().Contains(q) ||
                                     l.EntityName.ToLower().Contains(q) ||
                                     (l.Detail != null && l.Detail.ToLower().Contains(q)));
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .Select(l => new AuditLogDto(
                l.Id,
                l.UserFullName,
                l.Username,
                l.Action,
                l.EntityName,
                l.EntityId,
                l.Detail,
                l.IpAddress,
                l.CreatedAt
            ))
            .ToListAsync();
    }
}