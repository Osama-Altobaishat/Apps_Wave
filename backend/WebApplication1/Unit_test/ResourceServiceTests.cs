using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BookingApi.Data;
using BookingApi.Models;
using BookingApi.DTOs;
using Xunit;

public class ResourceServiceTests
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

    private User SeedUser(AppDbContext db)
    {
        var user = new User
        {
            FullName = "Admin",
            Email = "admin@test.com",
            Password = "123456",
            Role = UserRole.Admin
        };

        db.Users.Add(user);
        db.SaveChanges();

        return user;
    }
    [Fact]
    public async Task CreateResource_Should_ReturnResource()
    {
        var (db, conn) = GetDb();
        var user = SeedUser(db);

        var service = new ResourceService(db);

        var request = new CreateResourceRequest
("Room A",
            "Test room")
        ;

        var result = await service.CreateAsync(user.Id, request);

        Assert.NotNull(result);
        Assert.Equal("Room A", result.Name);

        var dbResource = await db.Resources.FindAsync(result.Id);
        Assert.NotNull(dbResource);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task GetAll_Should_ReturnOnlyActive()
    {
        var (db, conn) = GetDb();

        var user = SeedUser(db);

        db.Resources.AddRange(
            new Resource { Name = "A", IsActive = true, CreatedByUserId = user.Id },
            new Resource { Name = "B", IsActive = false, CreatedByUserId = user.Id }
        );

        db.SaveChanges();

        var service = new ResourceService(db);

        var result = await service.GetAllAsync(true);

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task GetById_Should_ReturnResource()
    {
        var (db, conn) = GetDb();
        var user = SeedUser(db);

        var resource = new Resource
        {
            Name = "Room A",
            CreatedByUserId = user.Id
        };

        db.Resources.Add(resource);
        db.SaveChanges();

        var service = new ResourceService(db);

        var result = await service.GetByIdAsync(resource.Id);

        Assert.NotNull(result);
        Assert.Equal("Room A", result!.Name);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task Update_Should_ReturnFalse_WhenNotFound()
    {
        var (db, conn) = GetDb();

        var service = new ResourceService(db);

        var request = new UpdateResourceRequest
        ("X", "Y", true);

        var result = await service.UpdateAsync(999, request);

        Assert.False(result);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task Update_Should_UpdateResource()
    {
        var (db, conn) = GetDb();
        var user = SeedUser(db);

        var resource = new Resource
        {
            Name = "Old",
            CreatedByUserId = user.Id
        };

        db.Resources.Add(resource);
        db.SaveChanges();

        var service = new ResourceService(db);

        var request = new UpdateResourceRequest("New", "Updated", true);

        var result = await service.UpdateAsync(resource.Id, request);

        Assert.True(result);

        var updated = await db.Resources.FindAsync(resource.Id);
        Assert.Equal("New", updated!.Name);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task Delete_Should_ReturnFalse_WhenNotFound()
    {
        var (db, conn) = GetDb();

        var service = new ResourceService(db);

        var result = await service.DeleteAsync(999);

        Assert.False(result);

        db.Dispose();
        conn.Dispose();
    }
    [Fact]
    public async Task Delete_Should_RemoveResource()
    {
        var (db, conn) = GetDb();
        var user = SeedUser(db);

        var resource = new Resource
        {
            Name = "Room",
            CreatedByUserId = user.Id
        };

        db.Resources.Add(resource);
        db.SaveChanges();

        var service = new ResourceService(db);

        var result = await service.DeleteAsync(resource.Id);

        Assert.True(result);

        var deleted = await db.Resources.FindAsync(resource.Id);
        Assert.Null(deleted);

        db.Dispose();
        conn.Dispose();
    }
}