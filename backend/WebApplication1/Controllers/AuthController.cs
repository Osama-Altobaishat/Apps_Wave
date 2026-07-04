using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using BookingApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
   

    public AuthController(AppDbContext db, ITokenService tokenService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        var hasher = new PasswordHasher<User>();
        if (emailExists)
            return Conflict(new { message = "This is Email Exists." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.User
        };
        user.Password = hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(await IssueTokensAsync(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Unauthorized(new { message = "Incorrect password or email." });

        if (!user.IsActive)
            return Forbid();

        var hasher = new PasswordHasher<User>();

        var result = hasher.VerifyHashedPassword(
            user,
            user.Password,
            request.Password
        );

        if (result != PasswordVerificationResult.Success)
            return Unauthorized(new { message = "Incorrect password or email." });

        return Ok(await IssueTokensAsync(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        Console.WriteLine(user);


        if (user is null)
            return Ok();


        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Logged out successfully." });
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            double.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"));

        await _db.SaveChangesAsync();

        return new AuthResponse(
            user.Id, user.FullName, user.Email, user.Role,
            accessToken, expiresAt, refreshToken);
    }
}
