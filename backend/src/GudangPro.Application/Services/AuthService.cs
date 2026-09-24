using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GudangPro.Application.DTOs;
using GudangPro.Domain.Entities;
using GudangPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GudangPro.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserRequest request, string currentUsername, string? ipAddress);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request, string currentUsername, string? ipAddress);
    Task ResetPasswordAsync(int id, string newPassword, string currentUsername, string? ipAddress);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IAuditService _audit;

    public AuthService(AppDbContext db, IConfiguration config, IAuditService audit)
    {
        _db = db;
        _config = config;
        _audit = audit;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var user = await _db.Users
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            throw new UnauthorizedAccessException("Username atau password salah.");

        // FR-AUTH-04: Kunci akun setelah 5 kali gagal login
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            throw new InvalidOperationException($"Akun terkunci karena terlalu banyak percobaan gagal. Coba lagi dalam {remaining} menit.");
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Akun ini telah dinonaktifkan. Hubungi admin.");

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await _db.SaveChangesAsync();
                await _audit.LogAsync(user.Id, user.FullName, user.Username, "KunciAkun", "User", user.Id.ToString(), null, "Akun terkunci otomatis setelah 5 kali gagal login", ipAddress);
                throw new InvalidOperationException("Akun terkunci selama 15 menit karena 5 kali salah password berturut-turut.");
            }
            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Username atau password salah.");
        }

        // Login berhasil — reset counter gagal
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(user.Id, user.FullName, user.Username, "Login", "User", user.Id.ToString(), null, "Pengguna berhasil login", ipAddress);

        var token = GenerateJwtToken(user);
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." + user.Id;

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Role,
            user.IsActive,
            user.UserWarehouses.Select(uw => uw.WarehouseId).ToList(),
            user.LastLoginAt
        );

        return new LoginResponse(token, refreshToken, userDto);
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        // Simple refresh token format: token.userId
        var parts = refreshToken.Split('.');
        if (parts.Length != 2 || !int.TryParse(parts[1], out var userId))
            throw new UnauthorizedAccessException("Refresh token tidak valid.");

        var user = await _db.Users
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            throw new UnauthorizedAccessException("Pengguna tidak ditemukan.");

        var token = GenerateJwtToken(user);
        var newRefresh = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." + user.Id;

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Role,
            user.IsActive,
            user.UserWarehouses.Select(uw => uw.WarehouseId).ToList(),
            user.LastLoginAt
        );

        return new LoginResponse(token, newRefresh, userDto);
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var users = await _db.Users
            .Include(u => u.UserWarehouses)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.FullName,
            u.Role,
            u.IsActive,
            u.UserWarehouses.Select(uw => uw.WarehouseId).ToList(),
            u.LastLoginAt
        )).ToList();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, string currentUsername, string? ipAddress)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException($"Username '{request.Username}' sudah digunakan.");

        var user = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            Role = request.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };

        if (request.WarehouseIds != null)
        {
            foreach (var wid in request.WarehouseIds)
            {
                user.UserWarehouses.Add(new UserWarehouse { WarehouseId = wid });
            }
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
        await _audit.LogAsync(adminUser?.Id, adminUser?.FullName ?? currentUsername, currentUsername, "Membuat", "User", user.Id.ToString(), null, $"Menambahkan pengguna baru: {user.FullName} ({user.Role})", ipAddress);

        return new UserDto(user.Id, user.Username, user.FullName, user.Role, user.IsActive, request.WarehouseIds ?? [], null);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request, string currentUsername, string? ipAddress)
    {
        var user = await _db.Users
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new KeyNotFoundException("Pengguna tidak ditemukan.");

        user.FullName = request.FullName;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        if (request.WarehouseIds != null)
        {
            user.UserWarehouses.Clear();
            foreach (var wid in request.WarehouseIds)
            {
                user.UserWarehouses.Add(new UserWarehouse { UserId = user.Id, WarehouseId = wid });
            }
        }

        await _db.SaveChangesAsync();

        var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
        await _audit.LogAsync(adminUser?.Id, adminUser?.FullName ?? currentUsername, currentUsername, "Mengubah", "User", user.Id.ToString(), null, $"Memperbarui pengguna {user.FullName}", ipAddress);

        return new UserDto(user.Id, user.Username, user.FullName, user.Role, user.IsActive, request.WarehouseIds ?? [], user.LastLoginAt);
    }

    // FR-AUTH-05: Admin dapat mengatur ulang password pengguna
    public async Task ResetPasswordAsync(int id, string newPassword, string currentUsername, string? ipAddress)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            throw new KeyNotFoundException("Pengguna tidak ditemukan.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await _db.SaveChangesAsync();

        var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
        await _audit.LogAsync(adminUser?.Id, adminUser?.FullName ?? currentUsername, currentUsername, "Mengubah", "User", user.Id.ToString(), null, $"Reset password untuk pengguna {user.FullName}", ipAddress);
    }

    private string GenerateJwtToken(User user)
    {
        var secret = _config["Jwt:Key"] ?? "GudangPro-Super-Secret-Key-For-JWT-2026-Min-32-Chars!";
        var issuer = _config["Jwt:Issuer"] ?? "GudangPro";
        var audience = _config["Jwt:Audience"] ?? "GudangProClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("role", user.Role),
        };

        foreach (var uw in user.UserWarehouses)
        {
            claims.Add(new Claim("warehouseId", uw.WarehouseId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}