using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.User;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int TenantId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsApiUser { get; set; }
} 