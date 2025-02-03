using System.Text.Json.Serialization;

namespace IceSMS.API.Models.User;

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Is2FARequired { get; set; }
    public bool Is2FACompleted { get; set; }
    public string? EmailVerificationToken { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
} 