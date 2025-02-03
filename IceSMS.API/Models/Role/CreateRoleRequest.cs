using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Role;

public class CreateRoleRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = new();
} 