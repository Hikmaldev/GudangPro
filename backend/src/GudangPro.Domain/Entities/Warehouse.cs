namespace GudangPro.Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserWarehouse> UserWarehouses { get; set; } = [];

    public ICollection<StockLevel> StockLevels { get; set; } = [];

    public ICollection<StockTransaction> Transactions { get; set; } = [];
}