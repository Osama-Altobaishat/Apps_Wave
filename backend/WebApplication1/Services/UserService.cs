using BookingApi.Data;
using BookingApi.DTOs;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await _db.Users
            .Select(u => new UserResponse(
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.IsActive,
                u.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> UpdateRoleAsync(int id, UpdateUserRoleRequest request)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return false;

        user.Role = request.Role;

        await _db.SaveChangesAsync();

        return true;
    }
}