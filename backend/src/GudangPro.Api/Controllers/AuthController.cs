using GudangPro.Application.DTOs;
using GudangPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GudangPro.Api.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Login pengguna dengan username dan password (PRD §10.4, FR-AUTH-01).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _auth.LoginAsync(request, ClientIpAddress);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error processing login request");
            if (ex.Message.Contains("28P01") || ex.Message.Contains("password authentication failed", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(500, new { message = "Koneksi database Supabase gagal: Password database salah. Silakan periksa password database di Render Environment Variables." });
            }
            if (ex.Message.Contains("Connection refused") || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(500, new { message = "Koneksi database Supabase gagal: Database tidak dapat dihubungi. Pastikan host connection pooler benar." });
            }
            return StatusCode(500, new { message = $"Terjadi kesalahan server: {ex.Message}" });
        }
    }

    /// <summary>Perbarui access token via refresh token (PRD §10.4, FR-AUTH-02).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _auth.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}