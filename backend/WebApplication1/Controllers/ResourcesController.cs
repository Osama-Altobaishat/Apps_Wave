using System.Security.Claims;
using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/resources")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResourcesController(AppDbContext db)
    {
        _db = db;
    }

    // Admin get Resource
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ResourceResponse>>> GetAll([FromQuery] bool onlyActive = true)
    {
        var query = _db.Resources.AsQueryable();
        if (onlyActive) query = query.Where(r => r.IsActive);

        var resources = await query
            .Select(r => new ResourceResponse(
                r.Id, r.Name, r.Description,
                r.IsActive, r.CreatedByUserId, r.CreatedAt))
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("User")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<ResourceResponseUser>>> GetResourcesUser([FromQuery] bool onlyActive = true)
    {
        var query = _db.Resources.AsQueryable();
        if (onlyActive) query = query.Where(r => r.IsActive);

        var resources = await query
            .Select(r => new ResourceResponseUser(
                r.Id, r.Name, r.Description))
            .ToListAsync();

        return Ok(resources);
    }



    // Admin Find Resource
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResourceResponse>> GetById(int id)
    {
        var r = await _db.Resources.FindAsync(id);
        if (r is null) return NotFound();

        return Ok(new ResourceResponse(
            r.Id, r.Name, r.Description,
            r.IsActive, r.CreatedByUserId, r.CreatedAt));
    }

    // Admin Create Resource
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResourceResponse>> Create(CreateResourceRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var resource = new Resource
        {
            Name = request.Name,
            Description = request.Description,
            CreatedByUserId = userId
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        var response = new ResourceResponse(
            resource.Id, resource.Name, resource.Description, resource.IsActive, resource.CreatedByUserId, resource.CreatedAt);

        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, response);
    }
    // Admin Update Resource
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateResourceRequest request)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null) return NotFound();

        resource.Name = request.Name;
        resource.Description = request.Description;
        resource.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Admin Delete Resource
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null) return NotFound();

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    [HttpPut("active/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateActiveStatus(int id, UpdateActiveStatusRequest request)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource is null)
            return NotFound();

        resource.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
