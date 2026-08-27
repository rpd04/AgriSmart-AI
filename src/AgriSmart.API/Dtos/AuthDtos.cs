using System.ComponentModel.DataAnnotations;
using AgriSmart.Core.Entities;

namespace AgriSmart.API.Dtos;

public record RegisterRequest(
    [Required] string Name,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password,
    string? Phone,
    string? Region,
    UserRole Role = UserRole.Farmer
);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(string Token, DateTime ExpiresAt, UserResponse User);

public record UserResponse(int UserId, string Name, string Email, string Role, string? Region);
