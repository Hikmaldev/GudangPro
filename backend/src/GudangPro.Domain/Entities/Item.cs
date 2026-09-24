namespace GudangPro.Domain.Entities;

public class Item
{
    public int Id { get; set; }

    /// <summary>Kode barang unik (FR-MST-02).</summary>
    public required string Code { get; set; }

    public required string Name { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public int UnitId { get; set; }

    public Unit Unit { get; set; } = null!;

    /// <summary>FR-MST-05: sistem tidak menghapus barang yang sudah punya transaksi, hanya menonaktifkan.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<StockLevel> StockLevels { get; set; } = [];

    public ICollection<StockTransactionLine> TransactionLines { get; set; } = [];

    public ICollection<Alert> Alerts { get; set; } = [];
}