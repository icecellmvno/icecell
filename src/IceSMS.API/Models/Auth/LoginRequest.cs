using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Auth;

public class LoginRequest
{
    [Required] public string? Identity { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;
}