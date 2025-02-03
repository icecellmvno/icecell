using System.Text.Json;
using IceSMS.API.Models.User;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace IceSMS.API.Services;

public class RedisSessionService : ISessionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly string _instanceName;
    private readonly int _sessionTimeout;

    public RedisSessionService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redis = redis;
        _db = redis.GetDatabase(configuration.GetValue<int>("Redis:DefaultDatabase"));
        _instanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "IceSMS_";
        _sessionTimeout = configuration.GetValue<int>("Redis:SessionTimeoutMinutes");
    }

    private string GetKey(string sessionId) => $"{_instanceName}session:{sessionId}";
    private string GetUserKey(int userId) => $"{_instanceName}user:{userId}:sessions";

    public async Task<Session> CreateSessionAsync(int userId, int tenantId, string ipAddress, string userAgent, bool is2FARequired = false)
    {
        var session = new Session
        {
            UserId = userId,
            TenantId = tenantId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Is2FARequired = is2FARequired,
            Is2FACompleted = !is2FARequired,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_sessionTimeout),
            RefreshToken = Guid.NewGuid().ToString()
        };

        var key = GetKey(session.Id);
        var userKey = GetUserKey(userId);

        var transaction = _db.CreateTransaction();
        
        _ = transaction.StringSetAsync(key, JsonSerializer.Serialize(session), TimeSpan.FromMinutes(_sessionTimeout));
        _ = transaction.SetAddAsync(userKey, session.Id);
        
        if (!await transaction.ExecuteAsync())
        {
            throw new Exception("Failed to create session");
        }

        return session;
    }

    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        var value = await _db.StringGetAsync(GetKey(sessionId));
        if (!value.HasValue) return null;

        var session = JsonSerializer.Deserialize<Session>(value!);
        return session?.IsExpired == true ? null : session;
    }

    public async Task<bool> UpdateSessionAsync(Session session)
    {
        var key = GetKey(session.Id);
        return await _db.StringSetAsync(key, JsonSerializer.Serialize(session), TimeSpan.FromMinutes(_sessionTimeout));
    }

    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null) return false;

        var key = GetKey(sessionId);
        var userKey = GetUserKey(session.UserId);

        var transaction = _db.CreateTransaction();
        _ = transaction.KeyDeleteAsync(key);
        _ = transaction.SetRemoveAsync(userKey, sessionId);

        return await transaction.ExecuteAsync();
    }

    public async Task<bool> DeleteUserSessionsAsync(int userId)
    {
        var userKey = GetUserKey(userId);
        var sessionIds = await _db.SetMembersAsync(userKey);

        if (!sessionIds.Any()) return true;

        var transaction = _db.CreateTransaction();
        foreach (var sessionId in sessionIds)
        {
            _ = transaction.KeyDeleteAsync(GetKey(sessionId.ToString()));
        }
        _ = transaction.KeyDeleteAsync(userKey);

        return await transaction.ExecuteAsync();
    }

    public async Task<IEnumerable<Session>> GetUserSessionsAsync(int userId)
    {
        var userKey = GetUserKey(userId);
        var sessionIds = await _db.SetMembersAsync(userKey);
        var sessions = new List<Session>();

        foreach (var sessionId in sessionIds)
        {
            var session = await GetSessionAsync(sessionId.ToString());
            if (session != null && !session.IsExpired)
            {
                sessions.Add(session);
            }
        }

        return sessions;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string sessionId, string refreshToken)
    {
        var session = await GetSessionAsync(sessionId);
        return session?.RefreshToken == refreshToken && !session.IsExpired;
    }

    public async Task<bool> Complete2FAAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null) return false;

        session.Is2FACompleted = true;
        return await UpdateSessionAsync(session);
    }

    public async Task<bool> VerifyEmailAsync(string sessionId, string token)
    {
        var session = await GetSessionAsync(sessionId);
        if (session == null || session.EmailVerificationToken != token) return false;

        session.IsEmailVerified = true;
        session.EmailVerificationToken = null;
        return await UpdateSessionAsync(session);
    }
} 