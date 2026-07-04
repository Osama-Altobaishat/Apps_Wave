using BookingApi.Models;

namespace BookingApi.Services;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
