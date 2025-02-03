namespace IceSMS.API.Models.Domain;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsSmsVerificationEnabled { get; set; }
    public bool IsEmailVerificationEnabled { get; set; }
    public bool IsGoogleAuthEnabled { get; set; }
    public string? GoogleAuthSecretKey { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    public User User { get; set; } = null!;
} 