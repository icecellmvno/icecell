namespace IceSMS.API.Models.Domain;

public class PanelSettings
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;
} 