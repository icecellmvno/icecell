using IceSMS.API.Models.User;

namespace IceSMS.API.Interfaces;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(int userId, int tenantId, string ipAddress, string userAgent, bool is2FARequired = false);
    Task<Session?> GetSessionAsync(string sessionId);
    Task<bool> UpdateSessionAsync(Session session);
    Task<bool> DeleteSessionAsync(string sessionId);
    Task<bool> DeleteUserSessionsAsync(int userId);
    Task<IEnumerable<Session>> GetUserSessionsAsync(int userId);
    Task<bool> ValidateRefreshTokenAsync(string sessionId, string refreshToken);
    Task<bool> Complete2FAAsync(string sessionId);
    Task<bool> VerifyEmailAsync(string sessionId, string token);
} 