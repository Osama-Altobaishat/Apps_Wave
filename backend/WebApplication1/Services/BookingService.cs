using System.Data;
using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BookingResponse>> GetAllAsync(int userId, bool isAdmin)
    {
        var query = _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Resource)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(b => b.UserId == userId);

        return await query
            .Select(b => new BookingResponse(
                b.Id,
                b.UserId,
                b.User!.FullName,
                b.ResourceId,
                b.Resource!.Name,
                b.StartAt,
                b.EndAt,
                b.Status,
                b.Notes,
                b.CreatedAt))
            .ToListAsync();
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, int userId, bool isAdmin)
    {
        var b = await _db.Bookings
            .Include(x => x.User)
            .Include(x => x.Resource)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b is null) return null;
        if (!isAdmin && b.UserId != userId) return null;

        return new BookingResponse(
            b.Id,
            b.UserId,
            b.User!.FullName,
            b.ResourceId,
            b.Resource!.Name,
            b.StartAt,
            b.EndAt,
            b.Status,
            b.Notes,
            b.CreatedAt);
    }

    public async Task<(bool Success, string? Error, BookingResponse? Data)> CreateAsync(
        int userId,
        CreateBookingRequest request)
    {
        if (request.EndAt <= request.StartAt)
            return (false, "End time must be after start time", null);

        await using var transaction =
            await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var resource = await _db.Resources.FindAsync(request.ResourceId);
        if (resource is null || !resource.IsActive)
            return (false, "Resource not available", null);

        var hasOverlap = await _db.Bookings.AnyAsync(b =>
            b.ResourceId == request.ResourceId &&
            b.Status != BookingStatus.Cancelled &&
            request.StartAt < b.EndAt &&
            request.EndAt > b.StartAt);

        if (hasOverlap)
            return (false, "Time slot already booked", null);

        var booking = new Booking
        {
            UserId = userId,
            ResourceId = request.ResourceId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
            Status = BookingStatus.Pending
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);

        var response = new BookingResponse(
            booking.Id,
            booking.UserId,
            user!.FullName,
            booking.ResourceId,
            resource.Name,
            booking.StartAt,
            booking.EndAt,
            booking.Status,
            booking.Notes,
            booking.CreatedAt);

        await transaction.CommitAsync();

        return (true, null, response);
    }

    public async Task<bool> UpdateStatusAsync(int id, BookingStatus status)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return false;

        booking.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int id, int userId, bool isAdmin)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return false;

        if (!isAdmin && booking.UserId != userId)
            return false;

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }
}