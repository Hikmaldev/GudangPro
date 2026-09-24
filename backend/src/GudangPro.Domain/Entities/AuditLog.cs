namespace GudangPro.Domain.Entities;

/// <summary>
/// Audit trail perubahan data (FR-AUD-01..03: siapa, kapan, apa yang berubah, tidak dapat diubah/dihapus).
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public required string UserFullName { get; set; }

    public required string Username { get; set; }

    /// <summary>Aksi: "Membuat", "Mengubah", "Membatalkan", "Menonaktifkan", "Login", "Logout".</summary>
    public required string Action { get; set; }

    public required string EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Detail { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}