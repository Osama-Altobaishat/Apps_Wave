using System.ComponentModel.DataAnnotations;
using BookingApi.Models;

namespace BookingApi.DTOs;

public record CreateBookingRequest(
    [Required] int ResourceId,
    [Required] DateTime StartAt,
    [Required] DateTime EndAt,
    [MaxLength(300)] string? Notes
);

public record UpdateBookingStatusRequest(
    [Required] BookingStatus Status
);

public record BookingResponse(
    int Id,
    int UserId,
    string UserFullName,
    int ResourceId,
    string ResourceName,
    DateTime StartAt,
    DateTime EndAt,
    BookingStatus Status,
    string? Notes,
    DateTime CreatedAt
);

public record BookingResponseUser(
    int Id,
    int ResourceId,
    string ResourceName,
    DateTime StartAt,
    DateTime EndAt,
    BookingStatus Status
);