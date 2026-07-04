using BookingApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Admin view users
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    // Admin change role
    [HttpPut("role/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var result = await _userService.UpdateRoleAsync(id, request);

        if (!result)
            return NotFound();

        return NoContent();
    }
}