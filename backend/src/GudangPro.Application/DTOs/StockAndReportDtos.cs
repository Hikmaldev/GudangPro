namespace GudangPro.Application.DTOs;

public record StockLevelDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    int WarehouseId,
    string WarehouseName,
    int Quantity,
    int MinStock,
    string Status // "Aman", "Menipis", "Kritis"
);

public record StockCardEntryDto(
    DateTime Date,
    string TransactionNo,
    string Reference,
    int? Incoming,
    int? Outgoing,
    int Balance,
    string CreatedBy
);

public record StockCardDto(
    ItemDto Item,
    int WarehouseId,
    string WarehouseName,
    int OpeningBalance,
    int TotalIncoming,
    int TotalOutgoing,
    int ClosingBalance,
    List<StockCardEntryDto> Entries
);

public record AlertDto(
    int Id,
    int ItemId,
    string ItemCode,
    string ItemName,
    int WarehouseId,
    string WarehouseName,
    string Message,
    string Level,
    bool IsRead,
    DateTime CreatedAt
);

public record DashboardSummaryDto(
    int TotalItems,
    int TransactionsToday,
    int LowStockCount,
    long StockValue,
    double ItemDelta,
    double TransactionDelta,
    double ValueDelta
);

public record CategoryStockDto(
    string Name,
    int Value,
    double Percentage,
    string Color
);

public record TrendPointDto(
    string Label,
    int Incoming,
    int Outgoing
);

public record DashboardChartsDto(
    List<CategoryStockDto> CategoryStock,
    List<TrendPointDto> Trend
);

public record AuditLogDto(
    int Id,
    string UserFullName,
    string Username,
    string Action,
    string EntityName,
    string? EntityId,
    string? Detail,
    string? IpAddress,
    DateTime CreatedAt
);