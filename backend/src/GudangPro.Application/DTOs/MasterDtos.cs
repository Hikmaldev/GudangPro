namespace GudangPro.Application.DTOs;

public record ItemDto(
    int Id,
    string Code,
    string Name,
    int CategoryId,
    string CategoryName,
    int UnitId,
    string UnitName,
    int MinStock,
    int TotalQuantity,
    bool IsActive,
    Dictionary<int, int> StockByWarehouse
);

public record CreateItemRequest(
    string Code,
    string Name,
    int CategoryId,
    int UnitId,
    int MinStock,
    int? InitialWarehouseId = null,
    int? InitialQuantity = null
);

public record UpdateItemRequest(
    string Name,
    int CategoryId,
    int UnitId,
    int MinStock
);

public record WarehouseDto(
    int Id,
    string Code,
    string Name,
    string? Address,
    bool IsActive,
    int ItemCount,
    int TotalStock
);

public record CreateWarehouseRequest(
    string Code,
    string Name,
    string? Address
);

public record UpdateWarehouseRequest(
    string Name,
    string? Address,
    bool IsActive
);

public record CategoryDto(int Id, string Name);
public record UnitDto(int Id, string Name);