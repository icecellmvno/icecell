using System.ComponentModel.DataAnnotations;

namespace IceSMS.API.Models.Role;

public class UpdateRoleRequest
{
    [StringLength(50)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public List<string>? Permissions { get; set; }
} 