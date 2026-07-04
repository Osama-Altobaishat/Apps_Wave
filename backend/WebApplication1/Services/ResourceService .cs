using BookingApi.Data;
using BookingApi.DTOs;
using BookingApi.Models;
using Microsoft.EntityFrameworkCore;

public class ResourceService : IResourceService
{
    private readonly AppDbContext _db;

    public ResourceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ResourceResponse>> GetAllAsync(bool onlyActive)
    {
        var query = _db.Resources.AsQueryable();

        if (onlyActive)
            query = query.Where(r => r.IsActive);

        return await query
            .Select(r => new ResourceResponse(
                r.Id,
                r.Name,
                r.Description,
                r.IsActive,
                r.CreatedByUserId,
                r.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<ResourceResponseUser>> GetForUserAsync(bool onlyActive)
    {
        var query = _db.Resources.AsQueryable();

        if (onlyActive)
            query = query.Where(r => r.IsActive);

        return await query
            .Select(r => new ResourceResponseUser(
                r.Id,
                r.Name,
                r.Description))
            .ToListAsync();
    }

    public async Task<ResourceResponse?> GetByIdAsync(int id)
    {
        var r = await _db.Resources.FindAsync(id);

        if (r == null) return null;

        return new ResourceResponse(
            r.Id,
            r.Name,
            r.Description,
            r.IsActive,
            r.CreatedByUserId,
            r.CreatedAt);
    }

    public async Task<ResourceResponse> CreateAsync(int userId, CreateResourceRequest request)
    {
        var resource = new Resource
        {
            Name = request.Name,
            Description = request.Description,
            CreatedByUserId = userId,
            IsActive = true
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return new ResourceResponse(
            resource.Id,
            resource.Name,
            resource.Description,
            resource.IsActive,
            resource.CreatedByUserId,
            resource.CreatedAt);
    }

    public async Task<bool> UpdateAsync(int id, UpdateResourceRequest request)
    {
        var resource = await _db.Resources.FindAsync(id);

        if (resource == null)
            return false;

        resource.Name = request.Name;
        resource.Description = request.Description;
        resource.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var resource = await _db.Resources.FindAsync(id);

        if (resource == null)
            return false;

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateActiveStatusAsync(int id, bool isActive)
    {
        var resource = await _db.Resources.FindAsync(id);

        if (resource == null)
            return false;

        resource.IsActive = isActive;

        await _db.SaveChangesAsync();

        return true;
    }
}