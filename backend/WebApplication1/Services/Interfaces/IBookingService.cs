using BookingApi.DTOs;
using BookingApi.Models;

public interface IBookingService
{
    Task<List<BookingResponse>> GetAllAsync(int userId, bool isAdmin);
    Task<BookingResponse?> GetByIdAsync(int id, int userId, bool isAdmin);

    Task<(bool Success, string? Error, BookingResponse? Data)> CreateAsync(
        int userId,
        CreateBookingRequest request);

    Task<bool> UpdateStatusAsync(int id, BookingStatus status);

    Task<bool> CancelAsync(int id, int userId, bool isAdmin);
}