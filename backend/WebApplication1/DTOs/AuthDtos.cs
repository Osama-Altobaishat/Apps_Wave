using System.ComponentModel.DataAnnotations;
using BookingApi.Models;

namespace BookingApi.DTOs;

public record RegisterRequest(
    [Required, MaxLength(100)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshRequest(
    [Required] string RefreshToken
);

public record AuthResponse(
    int UserId,
    string FullName,
    string Email,
    UserRole Role,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken
);

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateUserRoleRequest(
    [Required] UserRole Role
);
