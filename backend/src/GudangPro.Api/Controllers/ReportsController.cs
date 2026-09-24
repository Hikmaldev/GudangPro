using System.Text;
using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/reports")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IStockService _stock;
    private readonly ITransactionService _tx;

    public ReportsController(IStockService stock, ITransactionService tx)
    {
        _stock = stock;
        _tx = tx;
    }

    /// <summary>
    /// Ekspor laporan ke CSV (kompatibel Excel) sesuai PRD §10.4 / FR-HIS-06.
    /// Format: ?type=stock | ?type=transactions
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin Gudang,Pemilik")]
    public async Task<IActionResult> Export([FromQuery] string type = "stock", [FromQuery] int? warehouseId = null)
    {
        var sb = new StringBuilder();

        if (type.ToLower() == "transactions")
        {
            var txs = await _tx.GetTransactionsAsync(new TransactionFilter(WarehouseId: warehouseId, PageSize: 1000));
            sb.AppendLine("Nomor Transaksi,Jenis,Tanggal,Gudang,Referensi,Dibuat Oleh,Status,Total Item");
            foreach (var t in txs.Items)
            {
                sb.AppendLine($"\"{t.TransactionNo}\",\"{t.Type}\",\"{t.TransactionDate:yyyy-MM-dd}\",\"{t.WarehouseName}\",\"{t.ReferenceNo}\",\"{t.CreatedBy}\",\"{t.Status}\",{t.Lines.Count}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"laporan-transaksi-{DateTime.UtcNow:yyyyMMdd}.csv");
        }
        else
        {
            var stocks = await _stock.GetStockLevelsAsync(warehouseId);
            sb.AppendLine("Kode Barang,Nama Barang,Gudang,Stok Saat Ini,Stok Minimum,Status");
            foreach (var s in stocks)
            {
                sb.AppendLine($"\"{s.ItemCode}\",\"{s.ItemName}\",\"{s.WarehouseName}\",{s.Quantity},{s.MinStock},\"{s.Status}\"");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"laporan-stok-{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}