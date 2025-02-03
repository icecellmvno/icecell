using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Domain;

public class ContactGroup
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public List<Contact> Contacts { get; set; } = new();
} 