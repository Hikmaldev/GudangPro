namespace GudangPro.Domain.Entities;

/// <summary>Tabel relasi User–Warehouse (UserWarehouses).</summary>
public class UserWarehouse
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int WarehouseId { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}