using System.Security.Claims;
using BookingApi.Data;
using BookingApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // Admin view users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _db.Users
            .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    // Admin change role
    [HttpPut("role/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        user.Role = request.Role;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
