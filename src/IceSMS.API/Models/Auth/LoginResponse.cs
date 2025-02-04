namespace IceSMS.API.Models.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; }
    public bool Requires2FA { get; set; }
    public TwoFactorType? TwoFactorType { get; set; }
    public string? QrCodeUri { get; set; } // Google Auth için
}

public enum TwoFactorType
{
    Email,
    Sms,
    GoogleAuth
} 