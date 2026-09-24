using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/users")]
[Authorize(Roles = "Admin Gudang")]
public class UsersController : BaseApiController
{
    private readonly IAuthService _auth;

    public UsersController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _auth.GetUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var created = await _auth.CreateUserAsync(request, CurrentUsername, ClientIpAddress);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var updated = await _auth.UpdateUserAsync(id, request, CurrentUsername, ClientIpAddress);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // FR-AUTH-05: Admin dapat mengatur ulang password pengguna
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _auth.ResetPasswordAsync(id, request.NewPassword, CurrentUsername, ClientIpAddress);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}