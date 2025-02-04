namespace IceSMS.API.Models.Domain;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UInt64 Credit { get; set; }
    
    public int? ParentId { get; set; }

    public Tenant? Parent { get; set; }
    public List<Tenant> Children { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
} 