using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Domain;

public class Blacklist
{
    public int Id { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string? Reason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
} 