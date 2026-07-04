using System.ComponentModel.DataAnnotations;
using BookingApi.Models;

namespace BookingApi.DTOs;

public record CreateResourceRequest(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description
);

public record UpdateResourceRequest(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description,
    bool IsActive
);
public record UpdateActiveStatusRequest(
    bool IsActive
);

public record ResourceResponse(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int CreatedByUserId,
    DateTime CreatedAt
);
public record ResourceResponseUser(
    int Id,
    string Name,
    string? Description
);