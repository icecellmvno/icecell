using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.User;

public class UpdateUserRequest
{
    [StringLength(50)]
    public string? Username { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string>? Roles { get; set; }
    public bool? IsActive { get; set; }
} 