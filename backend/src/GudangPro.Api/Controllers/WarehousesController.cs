using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/warehouses")]
[Authorize]
public class WarehousesController : BaseApiController
{
    private readonly IInventoryService _inventory;

    public WarehousesController(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll([FromQuery] string? search)
    {
        var list = await _inventory.GetWarehousesAsync(search);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseRequest request)
    {
        try
        {
            var created = await _inventory.CreateWarehouseAsync(request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, [FromBody] UpdateWarehouseRequest request)
    {
        try
        {
            var updated = await _inventory.UpdateWarehouseAsync(id, request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}