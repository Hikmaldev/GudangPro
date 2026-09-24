using GudangPro.Domain.Enums;

namespace GudangPro.Domain.Entities;

public class StockTransaction
{
    public int Id { get; set; }

    /// <summary>Nomor unik transaksi, mis. TRX-260924-001.</summary>
    public required string TransactionNo { get; set; }

    public TransactionType Type { get; set; }

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    /// <summary>Nomor referensi eksternal, mis. nomor surat jalan.</summary>
    public required string ReferenceNo { get; set; }

    public string? Notes { get; set; }

    public required string CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Jika transaksi ini merupakan pembatalan/koreksi atas transaksi lain,
    /// kolom ini mengarah ke ID transaksi yang dibatalkan (FR-IN-05, FR-OUT-05, Aturan Bisnis #3).
    /// </summary>
    public int? CancelledOfId { get; set; }

    public StockTransaction? CancelledOf { get; set; }

    /// <summary>Alasan pembatalan jika transaksi ini membatalkan transaksi sebelumnya.</summary>
    public string? CancellationReason { get; set; }

    public bool IsCancelled { get; set; }

    public ICollection<StockTransactionLine> Lines { get; set; } = [];
}