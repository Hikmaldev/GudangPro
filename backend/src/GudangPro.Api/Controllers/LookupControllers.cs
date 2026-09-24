using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly IInventoryService _inventory;

    public CategoriesController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll() => Ok(await _inventory.GetCategoriesAsync());
}

[Route("api/[controller]")]
[Authorize]
public class UnitsController : BaseApiController
{
    private readonly IInventoryService _inventory;

    public UnitsController(IInventoryService inventory) => _inventory = inventory;

    [HttpGet]
    public async Task<ActionResult<List<UnitDto>>> GetAll() => Ok(await _inventory.GetUnitsAsync());
}