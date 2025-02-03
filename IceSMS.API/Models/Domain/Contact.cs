using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Domain;

public class Contact
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public List<ContactGroup> Groups { get; set; } = new();
    public List<CustomFieldValue> CustomFields { get; set; } = new();
} 