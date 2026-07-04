using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync(CurrentUserId, IsAdmin);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id, CurrentUserId, IsAdmin);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var result = await _service.CreateAsync(CurrentUserId, request);

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetById),
            new { id = result.Data!.Id },
            result.Data);
    }

    [HttpPut("status/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBookingStatusRequest request)
    {
        var ok = await _service.UpdateStatusAsync(id, request.Status);

        if (!ok) return NotFound();

        return NoContent();
    }

    [HttpPut("cancel/{id:int}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _service.CancelAsync(id, CurrentUserId, IsAdmin);

        if (!ok) return NotFound();

        return NoContent();
    }
}