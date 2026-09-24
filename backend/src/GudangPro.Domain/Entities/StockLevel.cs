using System.ComponentModel.DataAnnotations;

namespace GudangPro.Domain.Entities;

/// <summary>
/// Menyimpan saldo stok per barang per gudang beserta batas minimumnya.
/// Concurrency token menggunakan [Timestamp] untuk mapping ke SQL Server rowversion (§9.3).
/// </summary>
public class StockLevel
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Jumlah stok saat ini — tidak boleh bernilai negatif (Aturan Bisnis #2).</summary>
    public int Quantity { get; set; }

    /// <summary>Batas stok minimum per gudang (FR-VAL-01).</summary>
    public int MinStock { get; set; }

    /// <summary>Token concurrency optimistic SQL Server rowversion (§9.3, §10.3).</summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}