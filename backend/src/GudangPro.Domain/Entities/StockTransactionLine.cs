namespace GudangPro.Domain.Entities;

public class StockTransactionLine
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public StockTransaction Transaction { get; set; } = null!;

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    /// <summary>Jumlah barang. Harus positif (> 0) sesuai FR-IN-03.</summary>
    public int Quantity { get; set; }
}