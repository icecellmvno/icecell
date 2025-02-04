namespace IceSMS.API.Models.Domain;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Group { get; set; }

    public List<Role> Roles { get; set; } = new();
} 