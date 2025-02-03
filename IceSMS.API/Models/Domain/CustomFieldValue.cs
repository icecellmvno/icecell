namespace IceSMS.API.Models.Domain;

public class CustomFieldValue
{
    public int Id { get; set; }
    
    public string Value { get; set; } = string.Empty;
    
    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
    
    public int CustomFieldId { get; set; }
    public CustomField CustomField { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 