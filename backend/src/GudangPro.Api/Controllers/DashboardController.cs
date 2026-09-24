using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary([FromQuery] int? warehouseId)
    {
        var summary = await _dashboard.GetSummaryAsync(warehouseId);
        return Ok(summary);
    }

    [HttpGet("charts")]
    public async Task<ActionResult<DashboardChartsDto>> GetCharts([FromQuery] int? warehouseId)
    {
        var charts = await _dashboard.GetChartsAsync(warehouseId);
        return Ok(charts);
    }
}