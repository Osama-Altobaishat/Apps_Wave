using System.Security.Claims;
using BookingApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingApi.Controllers;

[ApiController]
[Route("api/resources")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _service;

    public ResourcesController(IResourceService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ResourceResponse>>> GetAll([FromQuery] bool onlyActive = true)
    {
        var result = await _service.GetAllAsync(onlyActive);
        return Ok(result);
    }

    [HttpGet("User")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<ResourceResponseUser>>> GetResourcesUser([FromQuery] bool onlyActive = true)
    {
        var result = await _service.GetForUserAsync(onlyActive);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResourceResponse>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResourceResponse>> Create(CreateResourceRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _service.CreateAsync(userId, request);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateResourceRequest request)
    {
        var result = await _service.UpdateAsync(id, request);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("active/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateActiveStatus(int id, UpdateActiveStatusRequest request)
    {
        var result = await _service.UpdateActiveStatusAsync(id, request.IsActive);

        if (!result)
            return NotFound();

        return NoContent();
    }
}