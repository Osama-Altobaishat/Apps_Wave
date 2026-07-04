using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using BookingApi.Data;
using BookingApi.Models;
using BookingApi.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using backend.Services.Interfaces;

public class AuthServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ITokenService> _tokenService;
    private readonly IConfiguration _config;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        _tokenService = new Mock<ITokenService>();

        _tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTime.UtcNow.AddHours(1)));

        _tokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
{
    { "Jwt:RefreshTokenDays", "7" }
})
            .Build();

        _service = new AuthService(_db, _tokenService.Object, _config);
    }

    // -------------------------
    // REGISTER SUCCESS
    // -------------------------
    [Fact]
    public async Task Register_Should_Create_User()
    {
        var request = new RegisterRequest("Osama","osama@test.com","123456");

        var result = await _service.Register(request);

        Assert.NotNull(result);
        Assert.Single(_db.Users);

        var user = await _db.Users.FirstAsync();

        Assert.Equal("Osama", user.FullName);
        Assert.Equal("osama@test.com", user.Email);
        Assert.NotNull(user.Password); // hashed
    }

    // -------------------------
    // REGISTER FAIL (EMAIL EXISTS)
    // -------------------------
    [Fact]
    public async Task Register_Should_Throw_When_Email_Exists()
    {
        _db.Users.Add(new User
        {
            FullName = "Existing",
            Email = "test@test.com",
            Password = "hashed"
        });

        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<Exception>(() =>
            _service.Register(new RegisterRequest("New","test@test.com","123456")));
    }

    // -------------------------
    // LOGIN SUCCESS
    // -------------------------
    [Fact]
    public async Task Login_Should_Return_Token_When_Correct()
    {
        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            FullName = "Osama",
            Email = "login@test.com",
            Role = UserRole.User,
            IsActive = true
        };

        user.Password = hasher.HashPassword(user, "123456");

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _service.Login(new LoginRequest("login@test.com","123456"));


        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    // -------------------------
    // LOGIN FAIL (WRONG PASSWORD)
    // -------------------------
    [Fact]
    public async Task Login_Should_Fail_When_Wrong_Password()
    {
        var hasher = new PasswordHasher<User>();

        var user = new User
        {
            FullName = "Osama",
            Email = "wrong@test.com",
            Role = UserRole.User,
            IsActive = true
        };

        user.Password = hasher.HashPassword(user, "correct-password");

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<Exception>(() =>
            _service.Login(new LoginRequest("wrong@test.com","wrong-password")));
    }

    // -------------------------
    // LOGOUT
    // -------------------------
    [Fact]
    public async Task Logout_Should_Clear_RefreshToken()
    {
        var user = new User
        {
            FullName = "Osama",
            Email = "logout@test.com",
            RefreshToken = "old-token"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _service.Logout("old-token");

        var updatedUser = await _db.Users.FirstAsync();

        Assert.Null(updatedUser.RefreshToken);
    }
}