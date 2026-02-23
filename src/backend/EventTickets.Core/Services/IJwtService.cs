using EventTickets.Core.Entities;

namespace EventTickets.Core.Services;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    string? GetEmailFromToken(string token);
}
