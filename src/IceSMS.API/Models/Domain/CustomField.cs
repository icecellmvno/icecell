using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Domain;

public enum CustomFieldType
{
    Text,
    Number,
    Date,
    Boolean,
    Select
}

public class CustomField
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public CustomFieldType Type { get; set; }
    
    public bool IsRequired { get; set; }
    
    public string? DefaultValue { get; set; }
    
    // Select tipi için seçenekler (JSON olarak saklanacak)
    public string? Options { get; set; }
    
    public int Order { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public List<CustomFieldValue> Values { get; set; } = new();
} 