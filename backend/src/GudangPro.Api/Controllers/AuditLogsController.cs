using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/audit-logs")]
[Authorize(Roles = "Admin Gudang")]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditService _audit;

    public AuditLogsController(IAuditService audit)
    {
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> GetAll([FromQuery] string? action, [FromQuery] string? search)
    {
        var logs = await _audit.GetLogsAsync(action, search);
        return Ok(logs);
    }
}