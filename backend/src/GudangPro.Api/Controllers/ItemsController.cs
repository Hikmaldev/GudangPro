using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/items")]
[Authorize]
public class ItemsController : BaseApiController
{
    private readonly IInventoryService _inventory;

    public ItemsController(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll([FromQuery] string? search, [FromQuery] string? status)
    {
        var items = await _inventory.GetItemsAsync(search, status);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        try
        {
            var item = await _inventory.GetItemByIdAsync(id);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemRequest request)
    {
        try
        {
            var created = await _inventory.CreateItemAsync(request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<ActionResult<ItemDto>> Update(int id, [FromBody] UpdateItemRequest request)
    {
        try
        {
            var updated = await _inventory.UpdateItemAsync(id, request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // FR-MST-05: Nonaktifkan barang (bukan hapus fisik)
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _inventory.DeactivateItemAsync(id, CurrentUsername, CurrentFullName, ClientIpAddress);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}