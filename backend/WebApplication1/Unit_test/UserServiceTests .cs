using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BookingApi.Data;
using BookingApi.Models;
using BookingApi.DTOs;
using Xunit;

public class UserServiceTests
{
    private (AppDbContext db, SqliteConnection conn) GetDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        return (db, connection);
    }
    [Fact]
    public async Task GetAll_Should_ReturnUsers()
    {
        var (db, conn) = GetDb();

        db.Users.Add(new User
        {
            FullName = "Osama",
            Email = "test@test.com",
            Password = "123",
            Role = UserRole.Admin,
            IsActive = true
        });

        await db.SaveChangesAsync();

        var service = new UserService(db);

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Osama", result[0].FullName);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task UpdateRole_Should_ChangeUserRole()
    {
        var (db, conn) = GetDb();

        var user = new User
        {
            FullName = "Osama",
            Email = "test@test.com",
            Password = "123",
            Role = UserRole.User
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new UserService(db);

        var request = new UpdateUserRoleRequest
        (
             UserRole.Admin
        );

        var result = await service.UpdateRoleAsync(user.Id, request);

        Assert.True(result);

        var updated = await db.Users.FindAsync(user.Id);
        Assert.Equal(UserRole.Admin, updated!.Role);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task UpdateRole_Should_ReturnFalse_WhenUserNotFound()
    {
        var (db, conn) = GetDb();

        var service = new UserService(db);

        var request = new UpdateUserRoleRequest
        (
            UserRole.User
        );

        var result = await service.UpdateRoleAsync(999, request);

        Assert.False(result);

        db.Dispose();
        conn.Dispose();
    }
}