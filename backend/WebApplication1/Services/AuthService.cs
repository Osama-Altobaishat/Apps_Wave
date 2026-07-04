using backend.Services.Interfaces;
using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, ITokenService tokenService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var exists = await _db.Users.AnyAsync(x => x.Email == request.Email);
        if (exists)
            throw new Exception("Email already exists");

        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.User
        };

        user.Password = hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return await IssueTokens(user);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            throw new Exception("Invalid credentials");

        var hasher = new PasswordHasher<User>();

        var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);

        if (result != PasswordVerificationResult.Success)
            throw new Exception("Invalid credentials");

        return await IssueTokens(user);
    }

    public async Task Logout(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        if (user == null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _db.SaveChangesAsync();
    }

    private async Task<AuthResponse> IssueTokens(User user)
    {
        var (token, expires) = _tokenService.GenerateAccessToken(user);
        var refresh = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            double.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"));

        await _db.SaveChangesAsync();

        return new AuthResponse(
            user.Id, user.FullName, user.Email, user.Role,
            token, expires, refresh);
    }
}