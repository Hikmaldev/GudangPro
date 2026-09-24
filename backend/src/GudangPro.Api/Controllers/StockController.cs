using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/stock")]
[Authorize]
public class StockController : BaseApiController
{
    private readonly IStockService _stock;

    public StockController(IStockService stock)
    {
        _stock = stock;
    }

    [HttpGet]
    public async Task<ActionResult<List<StockLevelDto>>> GetStock([FromQuery] int? warehouseId, [FromQuery] string? search)
    {
        var levels = await _stock.GetStockLevelsAsync(warehouseId, search);
        return Ok(levels);
    }

    // FR-HIS-05: Kartu stok per barang
    [HttpGet("{itemId}/card")]
    public async Task<ActionResult<StockCardDto>> GetStockCard(
        int itemId,
        [FromQuery] int warehouseId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var card = await _stock.GetStockCardAsync(itemId, warehouseId, startDate, endDate);
            return Ok(card);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}