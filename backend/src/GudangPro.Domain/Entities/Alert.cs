namespace GudangPro.Domain.Entities;

public class Alert
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public required string Message { get; set; }

    /// <summary>"kritis" jika Quantity &lt;= 0 atau &lt;= MinStock/2; "menipis" jika Quantity &lt;= MinStock.</summary>
    public string Level { get; set; } = "menipis";

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}