using GudangPro.Domain.Enums;

namespace GudangPro.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string PasswordHash { get; set; }

    public required string FullName { get; set; }

    /// <summary>Nilai peran sesuai enum <see cref="UserRole"/> (disimpan sebagai string, mis. "Admin Gudang").</summary>
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Jumlah gagal login beruntun (FR-AUTH-04: kunci akun setelah 5 kali gagal).</summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>Waktu sampai akun terkunci (FR-AUTH-04).</summary>
    public DateTime? LockoutEnd { get; set; }

    public DateTime? LastLoginAt { get; set; }

    /// <summary>Gudang yang ditugaskan (FR-HIS-01, aturan bisnis #5: staf hanya mengakses gudang yang ditugaskan).</summary>
    public ICollection<UserWarehouse> UserWarehouses { get; set; } = [];

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}