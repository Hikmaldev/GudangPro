using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/alerts")]
[Authorize]
public class AlertsController : BaseApiController
{
    private readonly IAlertService _alerts;

    public AlertsController(IAlertService alerts)
    {
        _alerts = alerts;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> GetAll([FromQuery] string? filter)
    {
        var list = await _alerts.GetAlertsAsync(filter);
        return Ok(list);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _alerts.MarkReadAsync(id);
        return NoContent();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _alerts.MarkAllReadAsync();
        return NoContent();
    }
}