using BookingApi.DTOs;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<bool> UpdateRoleAsync(int id, UpdateUserRoleRequest request);
}