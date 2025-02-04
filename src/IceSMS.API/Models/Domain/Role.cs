namespace IceSMS.API.Models.Domain;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<User> Users { get; set; } = new();
    public List<Permission> Permissions { get; set; } = new();
} 