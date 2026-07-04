using BookingApi.DTOs;

public interface IResourceService
{
    Task<List<ResourceResponse>> GetAllAsync(bool onlyActive);
    Task<List<ResourceResponseUser>> GetForUserAsync(bool onlyActive);
    Task<ResourceResponse?> GetByIdAsync(int id);
    Task<ResourceResponse> CreateAsync(int userId, CreateResourceRequest request);
    Task<bool> UpdateAsync(int id, UpdateResourceRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateActiveStatusAsync(int id, bool isActive);
}