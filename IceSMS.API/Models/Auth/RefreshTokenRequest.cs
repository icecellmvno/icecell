using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
} 