namespace GudangPro.Domain.Enums;

/// <summary>Jenis transaksi stok (PRD §10.3 — kolom Type pada StockTransactions).</summary>
public enum TransactionType
{
    IN = 1,
    OUT = 2,
    ADJUST = 3,
}

/// <summary>Peran pengguna (PRD §6 — RBAC).</summary>
public enum UserRole
{
    AdminGudang = 1,
    StafGudang = 2,
    Pemilik = 3,
}