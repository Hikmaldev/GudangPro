namespace GudangPro.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record UserDto(
    int Id,
    string Username,
    string FullName,
    string Role,
    bool IsActive,
    List<int> WarehouseIds,
    DateTime? LastLoginAt
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record RefreshTokenRequest(string RefreshToken);

public record CreateUserRequest(
    string Username,
    string Password,
    string FullName,
    string Role,
    List<int>? WarehouseIds
);

public record UpdateUserRequest(
    string FullName,
    string Role,
    bool IsActive,
    List<int>? WarehouseIds
);

public record ResetPasswordRequest(string NewPassword);