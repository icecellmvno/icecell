using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Auth;

public class ApiKeyRequest
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
} 