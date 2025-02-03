namespace IceSMS.API.Models.Auth;

public class ApiKeyResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } // Saniye cinsinden
    public int TenantId { get; set; }
} 