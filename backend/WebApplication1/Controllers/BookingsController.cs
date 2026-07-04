using System.Security.Claims;
using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
namespace BookingApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsStaff => User.IsInRole("Admin");

    //Admin Dashboard
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BookingResponse>>> GetAll()
    {
        var query = _db.Bookings
            .Include(b => b.User)
            .Include(b => b.Resource)
            .AsQueryable();

        if (!IsStaff)
            query = query.Where(b => b.UserId == CurrentUserId);

        var bookings = await query
            .Select(b => new BookingResponse(
                b.Id, b.UserId, b.User!.FullName, b.ResourceId, b.Resource!.Name,
                b.StartAt, b.EndAt, b.Status, b.Notes, b.CreatedAt))
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookingResponse>> GetById(int id)
    {
        var b = await _db.Bookings.Include(x => x.User).Include(x => x.Resource)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b is null) return NotFound();
        if (!IsStaff && b.UserId != CurrentUserId) return Forbid();

        return Ok(new BookingResponse(
            b.Id, b.UserId, b.User!.FullName, b.ResourceId, b.Resource!.Name,
            b.StartAt, b.EndAt, b.Status, b.Notes, b.CreatedAt));
    }

    [HttpGet("filter")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<BookingResponseUser>>> GetAll([FromQuery] int? resourceId = null)
    {

        Console.WriteLine(resourceId);
        Console.WriteLine("___________________________");
        var query = _db.Bookings
            .Include(b => b.Resource)
            .AsQueryable();

        if (resourceId.HasValue)
            query = query.Where(b => b.ResourceId == resourceId.Value);

        var result = await query
            .OrderByDescending(b => b.StartAt)
            .Select(b => new BookingResponseUser(
                b.Id,
                b.ResourceId,
                b.Resource!.Name,
                b.StartAt,
                b.EndAt,
                b.Status
            ))
            .ToListAsync();

        return Ok(result);
    }

    // User booking
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        if (request.EndAt <= request.StartAt)
            return BadRequest(new { message = "The end time must be after the start time." });

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var resource = await _db.Resources.FindAsync(request.ResourceId);
        if (resource is null || !resource.IsActive)
            return NotFound(new { message = "The resource does not exist or is unavailable." });

        var hasOverlap = await _db.Bookings.AnyAsync(b =>
            b.ResourceId == request.ResourceId &&
            b.Status != BookingStatus.Cancelled &&
            request.StartAt < b.EndAt &&
            request.EndAt > b.StartAt);

        if (hasOverlap)
            return Conflict(new { message = "This resource is already booked at this time." });

        var booking = new Booking
        {
            UserId = CurrentUserId,
            ResourceId = request.ResourceId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
            Status = BookingStatus.Pending
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(CurrentUserId);
        var response = new BookingResponse(
            booking.Id, booking.UserId, user!.FullName, booking.ResourceId, resource.Name,
            booking.StartAt, booking.EndAt, booking.Status, booking.Notes, booking.CreatedAt);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, response);
    }


    // Admin status
    [HttpPut("status/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBookingStatusRequest request)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();

        booking.Status = request.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Cancelled
    [HttpPut("Cancelled/{id:int}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();
        if (!IsStaff && booking.UserId != CurrentUserId) return Forbid();

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
