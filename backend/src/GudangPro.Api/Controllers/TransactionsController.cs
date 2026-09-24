using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/transactions")]
[Authorize]
public class TransactionsController : BaseApiController
{
    private readonly ITransactionService _txService;

    public TransactionsController(ITransactionService txService)
    {
        _txService = txService;
    }

    [HttpPost("in")]
    public async Task<ActionResult<StockTransactionDto>> CreateInbound([FromBody] CreateInboundTransactionRequest request)
    {
        try
        {
            var result = await _txService.CreateInboundAsync(request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("out")]
    public async Task<ActionResult<StockTransactionDto>> CreateOutbound([FromBody] CreateOutboundTransactionRequest request)
    {
        try
        {
            var result = await _txService.CreateOutboundAsync(request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // FR-IN-05, FR-OUT-05: Admin membatalkan transaksi (membuat transaksi koreksi)
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin Gudang")]
    public async Task<ActionResult<StockTransactionDto>> Cancel(int id, [FromBody] CancelTransactionRequest request)
    {
        try
        {
            var result = await _txService.CancelTransactionAsync(id, request, CurrentUsername, CurrentFullName, ClientIpAddress);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StockTransactionDto>>> GetAll([FromQuery] TransactionFilter filter)
    {
        var result = await _txService.GetTransactionsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockTransactionDto>> GetById(int id)
    {
        try
        {
            var tx = await _txService.GetTransactionByIdAsync(id);
            return Ok(tx);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}