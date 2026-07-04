using BookingApi.Models;

namespace backend.Services.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
