using Microsoft.Data.Sqlite;
using Xunit;
using BookingApi.Data;
using BookingApi.Models;
using BookingApi.DTOs;
using Microsoft.EntityFrameworkCore;

public class BookingServiceTests
{
    private (AppDbContext db, SqliteConnection connection) GetDb()
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

    private (User user, Resource resource) Seed(AppDbContext db)
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

        var resource = new Resource
        {
            Name = "Room A",
            CreatedByUserId = user.Id,
            IsActive = true
        };

        db.Resources.Add(resource);
        db.SaveChanges();

        return (user, resource);
    }

    [Fact]
    public async Task CreateBooking_Should_ReturnSuccess()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        var service = new BookingService(db);

        var request = new CreateBookingRequest(
            resource.Id,
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2),
            "Test booking"
        );

        var result = await service.CreateAsync(user.Id, request);

        Assert.True(result.Success);
        Assert.Null(result.Error);
        Assert.NotNull(result.Data);

        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public async Task CreateBooking_Should_Fail_WhenOverlapExists()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        db.Bookings.Add(new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(2),
            Status = BookingStatus.Pending
        });

        db.SaveChanges();

        var service = new BookingService(db);

        var request = new CreateBookingRequest(
            resource.Id,
            DateTime.UtcNow.AddMinutes(30),
            DateTime.UtcNow.AddHours(1),
            "Test booking"
        );

        var result = await service.CreateAsync(user.Id, request);

        Assert.False(result.Success);
        Assert.Equal("Time slot already booked", result.Error);

        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public async Task CreateBooking_Should_Fail_WhenEndBeforeStart()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        var service = new BookingService(db);

        var request = new CreateBookingRequest(
            resource.Id,
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(1),
            "Test booking"
        );

        var result = await service.CreateAsync(user.Id, request);

        Assert.False(result.Success);
        Assert.Equal("End time must be after start time", result.Error);

        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public async Task UpdateStatus_Should_ChangeStatus()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        var booking = new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.UpdateStatusAsync(booking.Id, BookingStatus.Completed);

        Assert.True(result);

        var updated = await db.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.Completed, updated!.Status);

        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public async Task Cancel_Should_SetCancelled()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        var booking = new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.CancelAsync(booking.Id, user.Id, false);

        Assert.True(result);

        var updated = await db.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, updated!.Status);

        db.Dispose();
        connection.Dispose();
    }

    [Fact]
    public async Task Cancel_Should_Fail_WhenNotOwnerOrAdmin()
    {
        var (db, connection) = GetDb();

        var (user, resource) = Seed(db);

        var booking = new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);
        db.SaveChanges();

        var service = new BookingService(db);

        var result = await service.CancelAsync(booking.Id, 999, false);

        Assert.False(result);

        var updated = await db.Bookings.FindAsync(booking.Id);
        Assert.Equal(BookingStatus.Pending, updated!.Status);

        db.Dispose();
        connection.Dispose();
    }
}