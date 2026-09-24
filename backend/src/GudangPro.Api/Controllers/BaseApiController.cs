using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected string CurrentUsername => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                       ?? User.FindFirst("unique_name")?.Value
                                       ?? User.Identity?.Name
                                       ?? "system";

    protected string CurrentFullName => User.FindFirst(ClaimTypes.Name)?.Value ?? CurrentUsername;

    protected string CurrentRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "Staf Gudang";

    protected string? ClientIpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
}