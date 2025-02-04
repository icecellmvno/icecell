namespace IceSMS.API.Models.Domain;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<Role> Roles { get; set; } = new();
    public UserProfile? Profile { get; set; }
} 