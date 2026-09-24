namespace GudangPro.Application.DTOs;

public record TransactionLineRequest(
    int ItemId,
    int Quantity
);

public record CreateInboundTransactionRequest(
    int WarehouseId,
    DateTime TransactionDate,
    string ReferenceNo,
    string? Notes,
    List<TransactionLineRequest> Lines
);

public record CreateOutboundTransactionRequest(
    int WarehouseId,
    DateTime TransactionDate,
    string ReferenceNo,
    string? Notes,
    List<TransactionLineRequest> Lines
);

public record CancelTransactionRequest(
    string Reason
);

public record TransactionLineDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    int Quantity,
    string UnitName,
    int AvailableStock
);

public record StockTransactionDto(
    int Id,
    string TransactionNo,
    string Type,
    int WarehouseId,
    string WarehouseName,
    DateTime TransactionDate,
    string ReferenceNo,
    string? Notes,
    string CreatedBy,
    DateTime CreatedAt,
    string Status,
    int? CancelledOfId,
    string? CancellationReason,
    List<TransactionLineDto> Lines
);

public record TransactionFilter(
    int? WarehouseId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Type = null,
    int? ItemId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);