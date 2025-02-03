using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Auth;

public class TwoFactorRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public TwoFactorType Type { get; set; }
} 